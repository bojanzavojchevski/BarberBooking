using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.Auth.DTOs;

public sealed record ResetPasswordDto(string Email, string Token, string NewPassword);
