# Roadmap

## Phase 0 — Scaffolding

- [ ] Solution .NET (`dotnet new sln`), projet compilateur (console app), projet de tests (xUnit).
- [ ] Pipeline CI minimal (build + tests) — optionnel, à discuter plus tard.

## Phase 1 — "Hello, world" (Linux)

- [ ] Lexer : tokenizer minimal (`print`, chaîne de caractères, fin d'instruction).
- [ ] Parser : AST pour une suite d'instructions `print "...";`.
- [ ] Génération de code : AST → C# → compilation Roslyn → assembly.
- [ ] `dotnet publish -r linux-x64 --self-contained` produit un binaire autonome qui exécute le `.tl`.
- [ ] Tests unitaires lexer/parser + test bout en bout (golden file sur la sortie stdout).

## Phase 2 — Extension du langage

- [ ] Variables, types simples (string, int, bool).
- [ ] Expressions (opérateurs arithmétiques/logiques).
- [ ] Structures de contrôle (if, while).
- [ ] Fonctions.

(Ordre et contenu à ajuster au fur et à mesure, en fonction de ce qu'on a envie de comprendre ensuite.)

## Phase 3 — Windows (self-contained, cross-compilé depuis Linux)

- [ ] `dotnet publish -r win-x64 --self-contained` depuis la machine Linux.
- [ ] Vérification du binaire Windows (VM ou CI avec runner Windows — impossible à exécuter depuis Linux directement).
- [ ] Documenter les différences éventuelles (chemins, fins de ligne, encodage console, etc.).

## Phase 4 — Exploration Rust (plus tard)

- [ ] Réécriture du compilateur en Rust une fois les bases bien comprises en C#.
- [ ] Comparaison des deux implémentations à but pédagogique.

## Backlog / idées non priorisées

- Native AOT si on veut un vrai binaire natif compilé (nécessite de builder sur l'OS cible, donc pas de cross-compilation possible).
- Messages d'erreur avec position (ligne/colonne).
- REPL.
