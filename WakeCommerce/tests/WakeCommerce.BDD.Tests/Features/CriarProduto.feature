Funcionalidade: Criar Produto
  Como um administrador do sistema
  Eu quero criar um novo produto
  Para que ele seja adicionado ao estoque

  Cenario: Criar produto com dados inválidos
    Dado que eu tenho um produto com o preco negativo
    Quando eu envio uma solicitação para criar o produto
    Entao eu devo receber um status de resposta 400
    E eu devo receber a mesangem "O preço não pode ser menor que 0"
