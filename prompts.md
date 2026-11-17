# Uso de IA e prompts utilizados

**Ferramenta:** Claude (Anthropic)
**Disciplina:** C# Software Development
**Escopo:** este arquivo cobre **apenas** o painel Windows Forms desta disciplina. A API REST tem repositório e `prompts.md` próprios, na disciplina de SOA.
**Regra da GS:** "IA como copiloto: permitida para aprender e revisar. Registrem os prompts em um arquivo prompts.md e saibam explicar cada linha."

## Como a IA foi usada

A ideia da CitizenTech **Gota Inteligente** foi definida em equipe, assim como o público do painel (Sindicato Rural e Prefeitura), as funcionalidades e as regras de priorização.

O desenvolvimento do painel foi conduzido por **Pedro Paulo**, que transformou esses requisitos nos pedidos registrados abaixo, executou o painel conectado à API da Gota Inteligente, conferiu cada tela e fluxo (cadastro, agendamento, filtros e modo offline), identificou os problemas, pediu as correções e revisou o código antes de cada entrega.

O Claude atuou como **copiloto**: gerou e revisou código a partir desses pedidos e ajudou na compilação, nos testes e na publicação no GitHub. Nenhum código foi entregue sem ser executado e revisado por Pedro Paulo.

Os prompts abaixo estão organizados por etapa e **redigidos de forma estruturada**. Cada um corresponde a um pedido realmente feito durante o desenvolvimento.

---

## 1. Requisitos e modelagem

> A partir da proposta da GS e da ideia **Gota Inteligente** (ESP32 com sensor de umidade, modelo de IA que decide a irrigação e app do agricultor), modele um painel em C# Windows Forms para o Sindicato Rural e a Prefeitura monitorarem o estresse hídrico das fazendas da região. Proponha as classes de domínio aplicando encapsulamento e separando domínio, serviços e telas.

**Resultado:** `FazendaMonitorada`, `TalhaoMonitorado`, `ResumoRegional` e a organização em `Dominio/`, `Servicos/`, `Ui/` e `Telas/`.

## 2. Fluxos de decisão

> Defina uma regra simples e explicável para priorizar o atendimento às fazendas (Urgente, Alta, Média e Baixa) a partir do pior nível de estresse dos talhões, com um critério de desempate. Para cada prioridade, sugira o tipo de atendimento e o prazo.

**Resultado:** `Prioridade`, `PontuacaoRisco` e `RecomendacaoAtendimento`.

## 3. Integração com a API e persistência

> Faça o painel consumir a API REST da Gota Inteligente, que já existe (`/api/painel/estresse-hidrico`, `/api/painel/resumo` e `/api/fazendas`), por meio de um cliente HTTP em C#. Se a API estiver fora do ar, o painel deve continuar funcionando com o último carregamento salvo. Os atendimentos técnicos devem ser persistidos localmente em JSON.

**Resultado (código C# do painel):** `ApiGotaInteligente`, `ServicoPainel`, `CachePainel` e `RepositorioAtendimentos`.

## 4. Telas e validações

> Crie as telas em Windows Forms: painel com indicadores e tabelas coloridas por prioridade, cadastro de fazenda e agendamento de atendimento. As validações devem aparecer ao lado de cada campo. Use linguagem simples e bom contraste, pensando em quem não é especialista.

**Resultado:** `FormPrincipal`, `FormCadastroFazenda`, `FormAtendimento` (herdam de `FormBase`), `CardIndicador` e `BarraDistribuicao`.

## 5. Teste e correção

> Ao cadastrar uma fazenda nova, ela não aparece no painel. Investigue a causa e corrija no próprio painel, sem alterar a API.

**Resultado:** o `/api/painel` só retorna fazendas com talhões; o painel passou a mesclar os dados com `/api/fazendas`, mostrando as novas como "Sem dados".

## 6. Ajustes de usabilidade

> Revise as telas: o texto do cabeçalho dos formulários aparece cortado, os rótulos não estão alinhados aos campos e as notas dos cards não cabem em janelas menores.

**Resultado:** ajustes de layout no `FormBase`, no `CardIndicador` e nos textos dos cards.

## 7. Versionamento

> Organize o versionamento com versionamento semântico: commits no padrão Conventional Commits em uma branch de feature, pull request para a `main`, tag `v1.0.0` e CHANGELOG.

**Resultado:** branch `feature/painel-estresse-hidrico`, PR #1, tag e release `v1.0.0`.

## 8. Documentação e entrega

> Escreva o README com o problema, a pessoa beneficiada, as instruções de execução, as tecnologias, os prints e a equipe. Gere também o zip do projeto e o arquivo txt de entrega.

**Resultado:** `README.md`, `docs/prints/` e `ENTREGA.txt`.
