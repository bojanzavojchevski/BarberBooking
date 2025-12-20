using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarberBooking.Application.Auth.DTOs;

public readonly record struct PasswordCheckResultDto(bool Succeeded, bool IsLockedOut);
