# RotaMoz — Gestão e consulta de transportes semi-colectivos

**Projecto académico | Instituto Superior Dom Bosco | 3.º ano**  
**Grupo indicado:** Cleide e Rijal  
**Tecnologias:** C#, ASP.NET Web Forms, .NET Framework 4.8, MySQL, Bootstrap 5 e Leaflet.

O RotaMoz organiza informação de rotas, paragens, transportadoras, veículos, motoristas, horários e tarifas, com foco inicial em Maputo e Matola. A área pública permite pesquisar trajectos; a área administrativa permite gerir os registos e exportar relatórios.

## Âmbito e limitações

O mapa apresenta percursos com coordenadas aproximadas e dados de demonstração. Não existe rastreamento GPS em tempo real, localização de motoristas, cálculo de tráfego ou integração com operadores. Os registos de exemplo são fictícios e carecem de confirmação local.

## Entrega

O pacote completo do projecto inclui a solução Web Forms, código C#, script MySQL, identidade visual, manual de desenvolvimento e memória descritiva. Foi entregue como arquivo ZIP nesta conversa. Este ramo contém a referência descritiva do projecto; não substitui o pacote completo.

## Ambiente e execução

É necessário Windows com .NET Framework 4.8 Developer Pack, ferramentas de compilação Web ASP.NET/MSBuild, IIS Express ou IIS e MySQL/MariaDB. O VS Code pode ser usado para editar os ficheiros, mas não fornece o designer visual Web Forms.

Antes de executar, importar Database/rotamoz.sql, configurar uma conta MySQL dedicada e actualizar a palavra-passe de exemplo no Web.config. Criar o primeiro administrador através de Configurar.aspx.

## Validação

Foram feitas verificações estáticas da estrutura XML, referências do projecto, delimitadores C# e eventos ASP.NET. A compilação e os testes funcionais têm ainda de ser executados num ambiente Windows com os componentes acima. Não apresentar esses testes como concluídos.

## Integridade académica

Os estudantes devem rever, adaptar e compreender o código, validar os dados e declarar a assistência técnica utilizada de acordo com as regras da instituição.