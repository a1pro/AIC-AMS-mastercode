namespace AicAms.Models.BaseResult
{
    public class OperationResult<T> : OperationResult
    {
        public T Data { get; set; }

        public static OperationResult<T> Result(ResultCode code, T data = default(T))
        {
            var op = new OperationResult<T>
            {
                Data = data,
                ResultCode = code
            };
            return op;
        }
    }
}
