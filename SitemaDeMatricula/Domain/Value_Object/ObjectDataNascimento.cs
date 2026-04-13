namespace SitemaDeMatricula.Domain.Value_Object
{
    public sealed class ObjectDataNascimento
    {
        public DateOnly Valor { get; }

        // Construtor privado: obriga o uso do método 'Criar'
        public ObjectDataNascimento(DateOnly valor) => Valor = valor;

        public static (ObjectDataNascimento? Data, string Error) Criar(DateOnly dataInput)
        {
            var hoje = DateOnly.FromDateTime(DateTime.Now);

            // 1. Validação de Futuro
            if (dataInput > hoje)
                return (null, "A data de nascimento não pode ser no futuro.");

            // 2. Validação de Idade Mínima (6 anos)
            int idade = hoje.Year - dataInput.Year;
            if (dataInput > hoje.AddYears(-idade)) idade--;

            if (idade < 6)
                return (null, "O aluno deve ter no mínimo 6 anos para ser matriculado.");

            // 3. Validação de consistência (ex: mais de 120 anos)
            if (idade > 120)
                return (null, "Data de nascimento inválida (idade limite excedida).");

            return (new ObjectDataNascimento(dataInput), string.Empty);
        }

        public ObjectDataNascimento()
        {
        }
    }
}