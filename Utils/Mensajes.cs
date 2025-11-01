namespace Utils
{
    public static class Mensajes
    {
        public static string RecuperarMensaje(CodigoMensajes codigo)
        {
            return codigo switch
            {
                CodigoMensajes.InternalServerError => "Error interno del servidor. Contacte al administrador.",
                CodigoMensajes.NotFound => "El recurso solicitado no fue encontrado.",
                CodigoMensajes.BadRequest => "Los datos enviados no son válidos.",
                CodigoMensajes.Unauthorized => "No tiene permisos para acceder a este recurso.",
                CodigoMensajes.DatabaseError => "Ocurrió un problema al acceder a la base de datos.",
                CodigoMensajes.ValidationError => "Algunos datos no cumplen con las validaciones requeridas.",
                CodigoMensajes.OperationSuccess => "La operación se realizó correctamente.",
                _ => "Ha ocurrido un error desconocido."
            };
        }
    }
}
