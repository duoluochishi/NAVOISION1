namespace NV.CT.Service.QualityTest.Models
{
    public class ResultModel
    {
        public ResultModel(bool success) : this(success, string.Empty)
        {
        }

        public ResultModel(bool success, string? message) : this(success, message, string.Empty)
        {
        }

        public ResultModel(bool success, string? message, string? code)
        {
            Success = success;
            Message = message;
            Code = code;
        }

        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Code { get; set; }

        public static ResultModel Create(bool success)
        {
            return new ResultModel(success);
        }

        public static ResultModel Create(bool success, string? message)
        {
            return new ResultModel(success, message);
        }

        public static ResultModel Create(bool success, string? message, string? code)
        {
            return new ResultModel(success, message, code);
        }
    }

    public class ResultModel<T> : ResultModel
    {
        public ResultModel(bool success, T data) : this(success, data, string.Empty, string.Empty)
        {
        }

        public ResultModel(bool success, string? message) : this(success, default, message, string.Empty)
        {
        }

        public ResultModel(bool success, string? message, string? code) : this(success, default, message, code)
        {
        }

        public ResultModel(bool success, T data, string? message, string? code) : base(success, message, code)
        {
            Result = data;
        }

        public T Result { get; set; }

        public static ResultModel<T> Create(bool success, T data)
        {
            return new ResultModel<T>(success, data);
        }

        public new static ResultModel<T> Create(bool success, string? message)
        {
            return new ResultModel<T>(success, message);
        }

        public new static ResultModel<T> Create(bool success, string? message, string? code)
        {
            return new ResultModel<T>(success, message, code);
        }

        public static ResultModel<T> Create(bool success, T data, string? message, string? code)
        {
            return new ResultModel<T>(success, data, message, code);
        }
    }
}