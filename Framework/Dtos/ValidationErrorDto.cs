namespace Checklists.Dtos
{
    public class ValidationErrorDto
    {
        public string ErrorCode { get; set; }
        public string Message { get; set; }

        public static ValidationErrorDto NullCreateInput = new ValidationErrorDto() { ErrorCode = "INPUT_NULL", Message = "Input object must not be null" };
        public static ValidationErrorDto NullUpdateInput =new ValidationErrorDto() { ErrorCode = "INPUT_NOT_UPDATABLE", Message = "Input object must not be null and must have valid id" };
        public static ValidationErrorDto MalformedInput = new ValidationErrorDto() { ErrorCode = "INPUT_MALFORMED", Message = "Input object could not be read" };
    }
}