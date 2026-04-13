using System.ComponentModel.DataAnnotations;

namespace SitemaDeMatricula.Aplicacao.Dtos.estudante;

public record EstudanteDtoCreate
(
  [Required(ErrorMessage = "O nome completo é obrigatório.")][MinLength(3, ErrorMessage = "O nome completo deve ter no mínimo 3 caracteres.")][MaxLength(80, ErrorMessage = "O nome completo deve ter no máximo 80 caracteres.")]
    string NomeCompleto,
    [Required(ErrorMessage = "O email é obrigatório.")][EmailAddress(ErrorMessage = "O email informado não é válido.")]
    string Email,
    [Required(ErrorMessage = "A data de nascimento é obrigatória.")]
    DateOnly DataNascimento,
    [Required(ErrorMessage = "O CPF é obrigatório.")][RegularExpression(@"^\d{11}$", ErrorMessage = "O CPF deve conter exatamente 11 números.")]
    string Cpf,
    [Required(ErrorMessage = "O telefone é obrigatório.")][Phone(ErrorMessage = "O telefone informado não é válido.")]
    string Telefone

);