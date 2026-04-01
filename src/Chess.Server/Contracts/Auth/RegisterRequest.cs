using System.ComponentModel.DataAnnotations;

namespace Chess.Server.Contracts.Auth;

public sealed record RegisterRequest(
    [Required, 
    StringLength(40, MinimumLength = 5), 
    DataType(DataType.Text)] 
    string Name, 
    [Required, 
    StringLength(100, MinimumLength = 6), 
    DataType(DataType.Password),
    RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", 
    ErrorMessage = "Must contain at least one uppercase letter, one lowercase letter, and one number.")] 
    string Password);
