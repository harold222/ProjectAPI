using System.ComponentModel.DataAnnotations;

namespace PostService.Application.DTOs;

public class PostDto
{
    public class Create
    {
        [Required(ErrorMessage = "El campo {PropertyName} es requerido")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "El campo {PropertyName} debe tener entre 1 y 500 caracteres")]
        public string Title { get; set; }

        [Required(ErrorMessage = "El campo {PropertyName} es requerido")]
        [StringLength(500, ErrorMessage = "El campo {PropertyName} no puede exceder 500 caracteres")]
        public string Body { get; set; }

        [Required(ErrorMessage = "El campo {PropertyName} es requerido")]
        public int CustomerId { get; set; }

        [StringLength(200, ErrorMessage = "El campo {PropertyName} no puede exceder 200 caracteres")]
        public string? CustomCategory { get; set; }

        public int? CategoryId { get; set; }
    }

    public class Update
    {
        [Required(ErrorMessage = "El campo {PropertyName} es requerido")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El campo {PropertyName} es requerido")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "El campo {PropertyName} debe tener entre 1 y 500 caracteres")]
        public string Title { get; set; }

        [Required(ErrorMessage = "El campo {PropertyName} es requerido")]
        [StringLength(500, ErrorMessage = "El campo {PropertyName} no puede exceder 500 caracteres")]
        public string Body { get; set; }

        public int? CategoryId { get; set; }

        [StringLength(200, ErrorMessage = "El campo {PropertyName} no puede exceder 200 caracteres")]
        public string? CustomCategory { get; set; }
    }

    public class DeleteResponse
    {
        public bool Status { get; set; }
    }

    public class Response
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public int? CategoryId { get; set; }
        public int CustomerId { get; set; }
        public bool Ok { get; set; } = true;
    }

    public class FailedItem
    {
        public string Title { get; set; }
        public int CustomerId { get; set; }
        public string Reason { get; set; }
        public bool Ok { get; set; } = false;
    }

    public class CreateAllResult
    {
        public List<Response> Created { get; set; } = new();
        public List<FailedItem> Failed { get; set; } = new();
    }
}
