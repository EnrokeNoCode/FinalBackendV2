namespace Model.DTO
{
    public class ApiRespuestaDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }

        public ApiRespuestaDTO(bool success, string message, object? data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }
        public static ApiRespuestaDTO Ok(string message, object? data = null)
            => new(true, message, data);

        public static ApiRespuestaDTO Error(string message)
            => new(false, message);
    }

}