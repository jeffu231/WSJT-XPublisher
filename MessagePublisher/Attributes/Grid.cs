using System.ComponentModel.DataAnnotations;

namespace MessagePublisher.Attributes;

public class Grid:RegularExpressionAttribute
{
    public Grid() : base("^[A-Za-z]{2}[1-9]{2}([A-Za-z]{2})?$")
    {
        ErrorMessage = "Grid must be a valid 4 or 6 character locator.";
    }
}