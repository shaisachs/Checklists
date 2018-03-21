
namespace Framework.Validators 
{
    public class ValidationError 
    {
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        
        public const string PropertyInvalidErrorCode = "PROPERTY_INVALID";
    }
}