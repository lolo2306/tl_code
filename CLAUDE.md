# TL — Compilateur d'un langage simple

## Contexte

Projet pédagogique : écrire son propre compilateur pour comprendre comment ça fonctionne, pas pour produire un langage de production.

- "TL" est un nom de travail (répertoire `tl`), à renommer si besoin.
- Cible : exécutable autonome (standalone), d'abord Linux, puis Windows en phase 2.
- v1 : une seule instruction, `print "..."`.

## Stack technique

- C#/.NET (dernière LTS installée sur la machine).
- Packaging via `dotnet publish --self-contained` (pas de Native AOT pour l'instant : la cross-compilation Linux → Windows ne marche qu'avec le self-contained classique, pas avec AOT qui exige de builder sur l'OS cible).

## Architecture (pipeline du compilateur)

Lexer → Parser → AST → Génération de code → exécutable autonome.

Décision à valider avec l'utilisateur au moment venu : le générateur de code transpile l'AST en C#, compilé via Roslyn (`Microsoft.CodeAnalysis.CSharp`), puis publié en self-contained. Une alternative plus "bas niveau" (émission IL directe, voire backend LLVM) peut être envisagée plus tard si l'objectif pédagogique évolue vers la génération de code natif.

## Documents de référence

- [docs/ROADMAP.md](docs/ROADMAP.md) — étapes du projet.
- [docs/TESTING.md](docs/TESTING.md) — conventions de tests.
- [docs/DOCUMENTATION.md](docs/DOCUMENTATION.md) — conventions de documentation.

## Conventions

- Pas de commentaires sauf si le "pourquoi" n'est pas évident (contournement, contrainte cachée, invariant surprenant).
- Pas de sur-ingénierie : on construit uniquement ce qui est nécessaire à l'étape en cours de la roadmap.
- Priorité à la compréhension : expliquer les choix de conception au fil de l'eau plutôt que les enfouir silencieusement dans le code.
- Remettre à jour le ROADMAP.md après chaque point terminé
