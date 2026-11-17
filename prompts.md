# Uso de IA e prompts utilizados

**Ferramenta:** Claude (Anthropic)
**Disciplina:** C# Software Development
**Escopo:** este arquivo cobre **apenas** o painel Windows Forms desta disciplina. A API REST tem repositório e `prompts.md` próprios, na disciplina de SOA.
**Regra da GS:** "IA como copiloto: permitida para aprender e revisar. Registrem os prompts em um arquivo prompts.md e saibam explicar cada linha."

## Como a IA foi usada

A ideia da CitizenTech **Gota Inteligente** foi definida em equipe, assim como o público do painel (Sindicato Rural e Prefeitura), as funcionalidades e as regras de priorização.

O código do painel foi escrito por **Pedro Paulo**, que também o executou conectado à API da Gota Inteligente, conferiu cada tela e fluxo (cadastro, agendamento, filtros e modo offline) e fez os testes.

O Claude atuou como **revisor**: Pedro Paulo enviou o código já escrito, e a ferramenta revisou, apontou problemas e fez pequenas alterações, além de ajudar na publicação no GitHub. Todas as alterações foram conferidas por Pedro Paulo antes da entrega.

> **Observação:** o histórico original da conversa com a ferramenta foi perdido e não pôde ser recuperado. Os prompts abaixo foram **reconstruídos de memória** por Pedro Paulo e redigidos de forma estruturada: representam o pedido feito em cada etapa, mas não são a transcrição literal.

---

## 1. Revisão da modelagem

> Escrevi as classes de domínio do painel Windows Forms (`FazendaMonitorada`, `TalhaoMonitorado` e `ResumoRegional`) e separei o projeto em `Dominio/`, `Servicos/`, `Ui/` e `Telas/`. Revise se o encapsulamento está correto e se a separação de responsabilidades entre as camadas faz sentido.

**Resultado:** sugestões de encapsulamento e pequenos ajustes nas classes de domínio.

## 2. Revisão das regras de priorização

> Implementei a regra de prioridade de atendimento (Urgente, Alta, Média e Baixa) a partir do pior nível de estresse dos talhões, com um critério de desempate. Revise a lógica e aponte casos em que a prioridade ou o prazo recomendado possam sair errados.

**Resultado:** revisão de `Prioridade`, `PontuacaoRisco` e `RecomendacaoAtendimento`.

## 3. Revisão da integração com a API e da persistência

> Segue o código que consome a API REST da Gota Inteligente (`/api/painel/estresse-hidrico`, `/api/painel/resumo` e `/api/fazendas`), o cache offline e a persistência dos atendimentos em JSON. Revise o tratamento de erros quando a API estiver fora do ar e verifique se os dados salvos localmente são carregados corretamente.

**Resultado:** ajustes pontuais em `ApiGotaInteligente`, `ServicoPainel`, `CachePainel` e `RepositorioAtendimentos`.

## 4. Revisão das telas e validações

> Revise as telas que eu criei (painel principal, cadastro de fazenda e agendamento de atendimento). Verifique se as validações aparecem ao lado de cada campo, se o contraste está bom e se a linguagem é clara para quem não é especialista.

**Resultado:** pequenas correções em `FormPrincipal`, `FormCadastroFazenda`, `FormAtendimento`, `CardIndicador` e `BarraDistribuicao`.

## 5. Depuração

> Ao cadastrar uma fazenda nova, ela não aparece no painel. Analise o meu código e aponte a causa, sem alterar a API.

**Resultado:** a causa foi identificada (o `/api/painel` só retorna fazendas com talhões), e o painel passou a mesclar os dados com `/api/fazendas`, mostrando as fazendas novas como "Sem dados".

## 6. Ajustes de usabilidade

> Revise o layout das telas: o texto do cabeçalho dos formulários aparece cortado, os rótulos não estão alinhados aos campos e as notas dos cards não cabem em janelas menores.

**Resultado:** ajustes de layout no `FormBase`, no `CardIndicador` e nos textos dos cards.

## 7. Versionamento

> Revise a organização do versionamento: commits no padrão Conventional Commits em uma branch de feature, pull request para a `main`, tag `v1.0.0` e CHANGELOG. Me ajude a publicar no GitHub.

**Resultado:** branch `feature/painel-estresse-hidrico`, PR #1, tag e release `v1.0.0`.

## 8. Revisão da documentação

> Revise o README (problema, pessoa beneficiada, instruções de execução, tecnologias, prints e equipe) e o arquivo de entrega, e aponte o que estiver faltando ou pouco claro.

**Resultado:** `README.md`, `docs/prints/` e `ENTREGA.txt` revisados.
