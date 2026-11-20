import { useCallback, useEffect, useState } from 'react'
import './App.css'
const K='bb_session'
const shopTime=x=>new Intl.DateTimeFormat('en-GB',{timeZone:'Europe/Skopje',dateStyle:'medium',timeStyle:'short'}).format(new Date(x))
const shopDay=x=>new Intl.DateTimeFormat('en-GB',{timeZone:'Europe/Skopje',weekday:'short',day:'numeric',month:'short'}).format(new Date(x+'T12:00:00Z'))
const today=()=>new Intl.DateTimeFormat('en-CA',{timeZone:'Europe/Skopje',year:'numeric',month:'2-digit',day:'2-digit'}).format(new Date())
const tokens=()=>{try{return JSON.parse(sessionStorage.getItem(K)||'null')}catch{return null}}
const claims=t=>{try{return JSON.parse(atob(t.split('.')[1].replace(/-/g,'+').replace(/_/g,'/')))}catch{return {}}}
const roles=t=>{const c=claims(t),r=c.role||c['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']||[];return Array.isArray(r)?r:[r]}
const money=n=>Number(n||0).toLocaleString('en-GB')+' MKD'
function App(){
 const [session,setSession]=useState(tokens),[page,setPage]=useState('discover'),[shops,setShops]=useState([]),[shop,setShop]=useState(null)
 const [query,setQuery]=useState(''),[service,setService]=useState(''),[barber,setBarber]=useState(''),[date,setDate]=useState(today)
 const [slots,setSlots]=useState([]),[slot,setSlot]=useState(''),[mine,setMine]=useState([]),[owner,setOwner]=useState({})
 const [auth,setAuth]=useState({email:'',password:''}),[register,setRegister]=useState(false),[newShop,setNewShop]=useState({name:'',slug:''})
 const [newBarber,setNewBarber]=useState({displayName:'',bio:''}),[newService,setNewService]=useState({name:'',durationMinutes:30,priceAmount:300})
 const [scheduleBarber,setScheduleBarber]=useState(''),[shifts,setShifts]=useState([]),[timeOff,setTimeOff]=useState({start:'',end:'',note:''})
 const [error,setError]=useState(''),[notice,setNotice]=useState(''),[busy,setBusy]=useState(false)
 const userRoles=session?roles(session.accessToken):[],isOwner=userRoles.includes('Owner'),isCustomer=userRoles.includes('Customer')
 const req=useCallback(async(path,options={},retry=true)=>{
  const current=tokens(),headers={...(options.body?{'Content-Type':'application/json'}:{}),...(current?{Authorization:'Bearer '+current.accessToken}:{})}
  const response=await fetch(path,{...options,headers:{...headers,...options.headers}})
  if(response.status===401&&current?.refreshToken&&retry){
   const refresh=await fetch('/api/auth/refresh',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({refreshToken:current.refreshToken})})
   if(refresh.ok){const t=await refresh.json();sessionStorage.setItem(K,JSON.stringify(t));setSession(t);return req(path,options,false)}
   sessionStorage.removeItem(K);setSession(null)
  }
  if(!response.ok){const body=await response.json().catch(()=>({}));throw Error(body.error||body.message||body.title||'HTTP '+response.status)}
  return response.status===204?null:response.json()
 },[])
 const perform=async(fn,message='')=>{setBusy(true);setError('');setNotice('');try{await fn();if(message)setNotice(message)}catch(e){setError(e.message)}finally{setBusy(false)}}
 const loadShops=useCallback(async(q='')=>{const d=await req('/api/public/shops?page=1&pageSize=30&q='+encodeURIComponent(q));setShops(d.items||[])},[req])
 const loadOwner=useCallback(async()=>{
  const results=await Promise.allSettled(['/api/owner/shop','/api/owner/barbers','/api/owner/services','/api/owner/bookings'].map(x=>req(x)))
  setOwner({shop:results[0].status==='fulfilled'?results[0].value:null,
   barbers:results[1].status==='fulfilled'?results[1].value:[],services:results[2].status==='fulfilled'?results[2].value:[],
   bookings:results[3].status==='fulfilled'?results[3].value:[]})
 },[req])
 useEffect(()=>{loadShops().catch(e=>setError(e.message))},[loadShops])
 useEffect(()=>{if(page==='mine'&&isCustomer)req('/api/bookings/mine').then(setMine).catch(e=>setError(e.message));if(page==='owner'&&isOwner)loadOwner().catch(e=>setError(e.message))},[page,isCustomer,isOwner,loadOwner,req])
 useEffect(()=>{
  if(!shop||!service||!barber||!date){setSlots([]);return}
  let stop=false;setSlot('')
  const p=new URLSearchParams({shopId:shop.shop.id,barberId:barber,serviceId:service,date})
  req('/api/public/availability?'+p).then(x=>{if(!stop)setSlots(x)}).catch(e=>{if(!stop)setError(e.message)})
  return()=>{stop=true}
 },[shop,service,barber,date,req])
 const openShop=slug=>perform(async()=>{
  const x=await req('/api/public/shops/'+encodeURIComponent(slug))
  setShop(x);setService(x.services[0]?.id||'');setBarber(x.barbers[0]?.id||'');setDate(today());setPage('discover')
  window.scrollTo({top:0,behavior:'smooth'})
 })
 const signIn=e=>{e.preventDefault();perform(async()=>{
  if(register)await req('/api/auth/register',{method:'POST',body:JSON.stringify(auth)})
  const t=await req('/api/auth/login',{method:'POST',body:JSON.stringify(auth)})
  sessionStorage.setItem(K,JSON.stringify(t));setSession(t);setPage('discover')
 },'Signed in successfully.')}
 const book=()=>{if(!session){setPage('account');return}if(!isCustomer){setError('A Customer account is required to book.');return}
  perform(async()=>{
   await req('/api/bookings',{method:'POST',body:JSON.stringify({shopId:shop.shop.id,barberId:barber,serviceId:service,startUtc:slot})})
   setMine(await req('/api/bookings/mine'));setPage('mine')
  },'Appointment confirmed.')}
 const cancel=(id,fromOwner=false)=>perform(async()=>{
  await req((fromOwner?'/api/owner/bookings/':'/api/bookings/')+id,{method:'DELETE'})
  if(fromOwner)await loadOwner();else setMine(await req('/api/bookings/mine'))
 },'Appointment cancelled.')
 const create=(path,value,reset)=>perform(async()=>{
  await req(path,{method:'POST',body:JSON.stringify(value)});reset();await loadOwner();await loadShops()
 },'Saved.')
 const selectSchedule=id=>perform(async()=>{setScheduleBarber(id);setShifts(id?await req('/api/owner/barbers/'+id+'/schedule'):[])})
 const saveShifts=()=>perform(async()=>{await req('/api/owner/barbers/'+scheduleBarber+'/schedule',{method:'PUT',body:JSON.stringify({shifts})})},'Working hours updated.')
 const dates=Array.from({length:14},(_,i)=>{const d=new Date();d.setDate(d.getDate()+i);return [d.getFullYear(),String(d.getMonth()+1).padStart(2,'0'),String(d.getDate()).padStart(2,'0')].join('-')})
 return <div className="shell">
  <header className="header"><div className="header-inner"><button className="logo" onClick={()=>{setShop(null);setPage('discover')}}><span className="logo-icon">✂</span> BARBER<span>BOOKING</span></button>
  <nav><button className={page==='discover'?'active':''} onClick={()=>setPage('discover')}>Discover</button>{isCustomer&&<button className={page==='mine'?'active':''} onClick={()=>setPage('mine')}>My bookings</button>}{isOwner&&<button className={page==='owner'?'active':''} onClick={()=>setPage('owner')}>Dashboard</button>}<button className="nav-sign" onClick={()=>setPage('account')}>{session?'Account':'Sign in'}</button></nav></div></header>
  {error&&<div className="toast error" role="alert" onClick={()=>setError('')}>{error} ×</div>}{notice&&<div className="toast success" role="status" onClick={()=>setNotice('')}>{notice} ×</div>}
  {page==='discover'&&!shop&&<>
   <section className="hero"><div className="hero-content"><div className="eyebrow">THE GOOD CUT CLUB <span>✦</span> SKOPJE</div><h1>LOOK SHARP.<br/><em>BOOK SMART.</em></h1><p>The best barbers are a few clicks away. Find your spot, choose your cut, and lock in a time.</p>
   <form className="search" onSubmit={e=>{e.preventDefault();perform(()=>loadShops(query))}}><span className="search-symbol">⌕</span><input aria-label="Find a barber shop" value={query} onChange={e=>setQuery(e.target.value)} placeholder="Find a barber shop…"/><button disabled={busy}>SEARCH SHOPS ↗</button></form>
   <div className="hero-tags"><span>✦ QUALITY CUTS</span><span>✦ EASY BOOKING</span><span>✦ NO PHONE CALLS</span></div></div><div className="hero-graphic" aria-hidden="true"><span>✂</span></div></section>
   <main className="container"><div className="section-title"><div><div className="eyebrow">FIND YOUR NEXT LOOK</div><h2>Explore barber shops<span className="lime">.</span></h2></div><small>{shops.length} shops</small></div>
   {shops.length?<div className="shop-grid">{shops.map((s,i)=><button className="shop-card" key={s.id} onClick={()=>openShop(s.slug)}><div className={'shop-art c'+i%4}><span>✂</span><small>{String(i+1).padStart(2,'0')}</small></div><div className="shop-info"><div><h3>{s.name}</h3><p>Barbershop · Book online</p></div><span className="arrow">↗</span></div></button>)}</div>:<div className="empty">No shops found. Owners can create a shop from their dashboard.</div>}</main>
  </>}
  {page==='discover'&&shop&&<>
   <section className="detail-hero"><button className="text-link" onClick={()=>setShop(null)}>← Back to shops</button><div className="eyebrow">YOUR NEXT APPOINTMENT STARTS HERE</div><h1>{shop.shop.name}<span className="lime">.</span></h1><p>Pick your service, barber and the perfect time.</p></section>
   <main className="booking-grid"><div><div className="step"><span>01</span><div><h2>Choose your service</h2><p>Every great cut starts here.</p></div></div>
    <div className="choice-grid">{shop.services.map(s=><button key={s.id} className={'choice '+(service===s.id?'selected':'')} onClick={()=>setService(s.id)}><span className="check">{service===s.id?'✓':''}</span><span><strong>{s.name}</strong><small>{s.durationMinutes} minutes</small></span><b>{money(s.priceAmount)}</b></button>)}</div>
    <div className="step"><span>02</span><div><h2>Pick your barber</h2><p>Find your perfect match.</p></div></div>
    <div className="choice-grid">{shop.barbers.map((b,i)=><button key={b.id} className={'choice '+(barber===b.id?'selected':'')} onClick={()=>setBarber(b.id)}><span className={'avatar c'+i%4}>{b.displayName[0]}</span><span><strong>{b.displayName}</strong><small>{b.bio||'Professional barber'}</small></span><span className="check">{barber===b.id?'✓':''}</span></button>)}</div>
    <div className="step"><span>03</span><div><h2>Find your time</h2><p>Times are shown in Skopje local time.</p></div></div>
    <div className="dates">{dates.map(d=><button key={d} className={d===date?'selected':''} onClick={()=>setDate(d)}><small>{shopDay(d)}</small><strong>{d.slice(-2)}</strong></button>)}</div>
    {!slots.length?<div className="empty">No available slots. Try another date or ask the owner to set working hours.</div>:<div className="slots">{slots.map(x=><button key={x.startUtc} className={slot===x.startUtc?'selected':''} onClick={()=>setSlot(x.startUtc)}>{new Intl.DateTimeFormat('en-GB',{timeZone:'Europe/Skopje',hour:'2-digit',minute:'2-digit'}).format(new Date(x.startUtc))}</button>)}</div>}
   </div><aside className="summary"><div className="eyebrow">YOUR BOOKING</div><h2>Looking good.</h2>
    <div className="summary-row"><span>Shop</span><strong>{shop.shop.name}</strong></div><div className="summary-row"><span>Service</span><strong>{shop.services.find(x=>x.id===service)?.name||'—'}</strong></div><div className="summary-row"><span>Barber</span><strong>{shop.barbers.find(x=>x.id===barber)?.displayName||'—'}</strong></div><div className="summary-row"><span>When</span><strong>{slot?shopTime(slot):'Choose a time'}</strong></div>
    <div className="total"><span>Total</span><strong>{money(shop.services.find(x=>x.id===service)?.priceAmount)}</strong></div>
    <button className="primary full" disabled={!slot||busy} onClick={book}>{session?'CONFIRM BOOKING':'SIGN IN TO BOOK'} ↗</button><small className="help">No online payment. Customer cancellations are accepted until two hours before the appointment.</small></aside></main>
  </>}
  {page==='account'&&<main className="form-page"><div className="panel"><div className="eyebrow">YOUR ACCOUNT</div>{session?<><h1>You're in<span className="lime">.</span></h1><p>Signed in with roles: {userRoles.join(', ')}</p><button className="primary" onClick={()=>{sessionStorage.removeItem(K);setSession(null);setPage('discover')}}>SIGN OUT ↗</button></>:<><h1>{register?'Join the club':'Sign in'}<span className="lime">.</span></h1><p>Book your next fresh cut in minutes.</p><form className="form" onSubmit={signIn}><label>Email<input type="email" required autoComplete="email" value={auth.email} onChange={e=>setAuth({...auth,email:e.target.value})}/></label><label>Password<input type="password" required minLength={register?12:1} autoComplete={register?'new-password':'current-password'} value={auth.password} onChange={e=>setAuth({...auth,password:e.target.value})}/></label>{register&&<small>Use at least 12 characters.</small>}<button className="primary full" disabled={busy}>{register?'CREATE CUSTOMER ACCOUNT':'SIGN IN'} ↗</button></form><button className="text-link" onClick={()=>setRegister(!register)}>{register?'Already registered? Sign in':'New here? Create an account'}</button></>}</div></main>}
  {page==='mine'&&<main className="container page"><div className="eyebrow">FRESH CUTS AHEAD</div><h1>My bookings<span className="lime">.</span></h1>{mine.length?mine.map(b=><div className="appointment" key={b.id}><div><div className="eyebrow">{shopTime(b.startUtc)}</div><h3>{money(b.priceAmount)}</h3><small>Booking #{b.id.slice(0,8)}</small></div><span className={'status '+b.status.toLowerCase()}>{b.status}</span>{b.status==='Confirmed'&&<button className="outline" disabled={busy} onClick={()=>cancel(b.id)}>Cancel</button>}</div>):<div className="empty">No bookings yet. Explore shops to book your first appointment.</div>}</main>}
  {page==='owner'&&<main className="container page"><div className="eyebrow">YOUR STUDIO</div><h1>Owner dashboard<span className="lime">.</span></h1>
   {!owner.shop?<section className="panel"><h2>Create a shop</h2><form className="inline-form" onSubmit={e=>{e.preventDefault();create('/api/owner/shop',newShop,()=>setNewShop({name:'',slug:''}))}}><input placeholder="Shop name" minLength={2} required value={newShop.name} onChange={e=>setNewShop({...newShop,name:e.target.value})}/><input placeholder="shop-slug" minLength={3} pattern="[a-z0-9-]+" required value={newShop.slug} onChange={e=>setNewShop({...newShop,slug:e.target.value})}/><button className="primary">CREATE SHOP</button></form></section>:<>
    <div className="owner-intro"><div><div className="eyebrow">YOUR SHOP</div><h2>{owner.shop.name}</h2><p>Manage services, staff and appointments.</p></div><button className="outline" onClick={()=>openShop(owner.shop.slug)}>Public shop ↗</button></div>
    <div className="owner-cols"><section className="panel"><h2>Services · {owner.services?.length||0}</h2>{owner.services?.map(x=><div key={x.id} className="mini-row"><strong>{x.name}</strong><small>{x.durationMinutes} min</small></div>)}<h3>Add a service</h3><form className="form" onSubmit={e=>{e.preventDefault();create('/api/owner/services',newService,()=>setNewService({name:'',durationMinutes:30,priceAmount:300}))}}><label>Name<input required minLength={2} value={newService.name} onChange={e=>setNewService({...newService,name:e.target.value})}/></label><label>Duration (minutes)<input required type="number" step="5" min="5" max="480" value={newService.durationMinutes} onChange={e=>setNewService({...newService,durationMinutes:Number(e.target.value)})}/></label><label>Price (MKD)<input required type="number" min="0" step="0.01" value={newService.priceAmount} onChange={e=>setNewService({...newService,priceAmount:Number(e.target.value)})}/></label><button className="primary">ADD SERVICE ↗</button></form></section>
    <section className="panel"><h2>Barbers · {owner.barbers?.length||0}</h2>{owner.barbers?.map(x=><div key={x.id} className="mini-row"><strong>{x.displayName}</strong><small>Staff</small></div>)}<h3>Add a barber</h3><form className="form" onSubmit={e=>{e.preventDefault();create('/api/owner/barbers',newBarber,()=>setNewBarber({displayName:'',bio:''}))}}><label>Name<input required minLength={2} value={newBarber.displayName} onChange={e=>setNewBarber({...newBarber,displayName:e.target.value})}/></label><label>Bio<input maxLength={600} value={newBarber.bio} onChange={e=>setNewBarber({...newBarber,bio:e.target.value})}/></label><button className="primary">ADD BARBER ↗</button></form></section></div>
    <section className="panel"><div className="eyebrow">WORKING HOURS</div><h2>Barber schedule</h2><label className="field">Choose barber<select value={scheduleBarber} onChange={e=>selectSchedule(e.target.value)}><option value="">Select barber</option>{owner.barbers?.map(x=><option key={x.id} value={x.id}>{x.displayName}</option>)}</select></label>
    {!!scheduleBarber&&<>{shifts.map((x,i)=><div className="shift" key={i}><select aria-label="Day" value={x.dayOfWeek} onChange={e=>setShifts(a=>a.map((v,j)=>j===i?{...v,dayOfWeek:Number(e.target.value)}:v))}>{['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'].map((d,n)=><option key={d} value={n}>{d}</option>)}</select><input type="time" aria-label="Start" step="900" value={x.start} onChange={e=>setShifts(a=>a.map((v,j)=>j===i?{...v,start:e.target.value}:v))}/><span>to</span><input type="time" aria-label="End" step="900" value={x.end} onChange={e=>setShifts(a=>a.map((v,j)=>j===i?{...v,end:e.target.value}:v))}/><button className="text-link" onClick={()=>setShifts(a=>a.filter((_,j)=>j!==i))}>Remove</button></div>)}<div className="inline-form"><button className="outline" onClick={()=>setShifts([...shifts,{dayOfWeek:1,start:'09:00',end:'17:00'}])}>+ Add shift</button><button className="primary" disabled={busy} onClick={saveShifts}>SAVE HOURS ↗</button></div>
    <h3>Block time off</h3><div className="inline-form"><label className="field">Start<input type="datetime-local" value={timeOff.start} onChange={e=>setTimeOff({...timeOff,start:e.target.value})}/></label><label className="field">End<input type="datetime-local" value={timeOff.end} onChange={e=>setTimeOff({...timeOff,end:e.target.value})}/></label><label className="field">Note<input value={timeOff.note} onChange={e=>setTimeOff({...timeOff,note:e.target.value})}/></label><button className="outline" disabled={!timeOff.start||!timeOff.end||busy} onClick={()=>perform(async()=>{await req('/api/owner/barbers/'+scheduleBarber+'/time-off',{method:'POST',body:JSON.stringify({startUtc:new Date(timeOff.start).toISOString(),endUtc:new Date(timeOff.end).toISOString(),note:timeOff.note})});setTimeOff({start:'',end:'',note:''})},'Time off saved.')}>BLOCK TIME</button></div></>}</section>
    <section className="panel"><h2>Appointments · {owner.bookings?.length||0}</h2>{owner.bookings?.length?owner.bookings.map(b=><div className="appointment" key={b.id}><div><strong>{shopTime(b.startUtc)}</strong><p>{money(b.priceAmount)}</p></div><span className={'status '+b.status.toLowerCase()}>{b.status}</span>{b.status==='Confirmed'&&<button className="outline" onClick={()=>cancel(b.id,true)}>Cancel</button>}</div>):<div className="empty">No appointments yet.</div>}</section>
   </>}
  </main>}
  <footer><strong>✂ BARBERBOOKING</strong><span>LOOK SHARP. BOOK SMART.</span><span>© 2026 BARBERBOOKING</span></footer>
 </div>
}
export default App
