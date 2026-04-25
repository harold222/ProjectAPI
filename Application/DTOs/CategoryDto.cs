using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class CategoryDto
{
    public class Create
    {
        [Required(ErrorMessage = "El campo {PropertyName} es requerido")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "El campo {PropertyName} debe tener entre 1 y 200 caracteres")]
        public string CategoryName { get; set; }
    }

    public class Response
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
    }
}