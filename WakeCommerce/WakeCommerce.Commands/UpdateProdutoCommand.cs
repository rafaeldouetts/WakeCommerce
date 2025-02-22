using System.ComponentModel.DataAnnotations;

namespace WakeCommerce.Commands
{
    public class UpdateProdutoCommand
    {
        //[Required(ErrorMessage = "O nome é obrigatório.")]
        //[StringLength(30, ErrorMessage = "O nome deve ter no máximo 30 caracteres.")]
        public string Nome { get; set; }
        public string Descricao { get; set; }
        //[Required(ErrorMessage = "O Preco é obrigatório.")]
        //[Range(0, double.MaxValue, ErrorMessage = "O preço não pode ser negativo.")]
        public decimal Preco { get; set; }
        //[Required(ErrorMessage = "O Estoque é obrigatório.")]
        //[Range(0, int.MaxValue, ErrorMessage = "O estoque não pode ser negativo.")]
        public int Estoque { get; set; }
    }
}
