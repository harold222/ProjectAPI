using System.ComponentModel.DataAnnotations;

namespace CustomerService.Application.DTOs;

public class CustomerDto
{
    public class Create
    {
        [Required(ErrorMessage = "El campo {PropertyName} es requerido")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "El campo {PropertyName} debe tener entre 1 y 500 caracteres")]
        public string Name { get; set; }
    }

    public class Update
    {
        [Required(ErrorMessage = "El campo {PropertyName} es requerido")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {PropertyName} es requerido")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "El campo {PropertyName} debe tener entre 1 y 500 caracteres")]
        public string Name { get; set; }
    }

    public class DeleteResponse
    {
        public bool Status { get; set; }
    }

    /// <summary>
    /// Response usado internamente y expuesto al consumir el endpoint GET /customer?id=X desde PostService.
    /// </summary>
    public class Response
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Ok { get; set; } = true;
    }

    public class FailedItem
    {
        public string Name { get; set; }
        public string Reason { get; set; }
        public bool Ok { get; set; } = false;
    }

    public class CreateAllResult
    {
        public List<Response> Created { get; set; } = new();
        public List<FailedItem> Failed { get; set; } = new();
    }
}
