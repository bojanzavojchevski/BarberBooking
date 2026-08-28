import { useEffect, useState } from 'react'
import './App.css'

function App() {
  const [shopDetails, setShopDetails] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  useEffect(() => {
    async function loadShop() {
      try {
        const response = await fetch('/api/public/shops/fade3studio')

        if (!response.ok) {
          throw new Error(`Request failed with status ${response.status}`)
        }

        const data = await response.json()
        setShopDetails(data)
      } catch (err) {
        setError(err.message)
      } finally {
        setLoading(false)
      }
    }

    loadShop()
  }, [])

  if (loading) {
    return <p>Loading...</p>
  }

  if (error) {
    return <p>Error: {error}</p>
  }

  return (
    <main>
      <h1>BarberBooking</h1>

      <section>
        <h2>{shopDetails.shop.name}</h2>
      </section>

      <section>
        <h2>Services</h2>

        {shopDetails.services.length === 0 ? (
          <p>No services available.</p>
        ) : (
          shopDetails.services.map((service) => (
            <div key={service.id}>
              <h3>{service.name}</h3>
            </div>
          ))
        )}
      </section>

      <section>
        <h2>Barbers</h2>

        {shopDetails.barbers.length === 0 ? (
          <p>No barbers available.</p>
        ) : (
          shopDetails.barbers.map((barber) => (
            <div key={barber.id}>
              <h3>{barber.displayName}</h3>
              <p>{barber.bio}</p>
            </div>
          ))
        )}
      </section>
    </main>
  )
}

export default App