namespace NV.CT.Alg.ScanReconValidation.Model;

public class ValidationResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public ValidationType ValidationType { get; set; }

    public ValidationResult(ValidationType type, bool isSuccess = true)
    {
        ValidationType = type;
        IsSuccess = isSuccess;
    }
}
