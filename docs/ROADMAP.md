# Roadmap

## Phase 0 — Scaffolding ✅

- [x] Solution .NET (`dotnet new sln`), projet compilateur (console app), projet de tests (xUnit).
- [ ] Pipeline CI minimal (build + tests) — optionnel, à discuter plus tard.

## Phase 1 — "Hello, world" (Linux) ✅

- [x] Lexer : tokenizer minimal (`print`, chaîne de caractères, fin d'instruction).
- [x] Parser : AST pour une suite d'instructions `print "...";`.
- [x] Génération de code : AST → C# → compilation (via `dotnet publish`, qui utilise Roslyn) → assembly.
- [x] `dotnet publish -r linux-x64 --self-contained` produit un binaire autonome qui exécute le `.tl`.
- [x] Tests unitaires lexer/parser/codegen + test bout en bout qui compile et exécute réellement le binaire.

## Phase 2 — Extension du langage

- [x] Variables, types simples (string, int, bool) + messages d'erreur avec position (ligne/colonne).
- [ ] Expressions (opérateurs arithmétiques/logiques).
- [ ] Structures de contrôle (if, while).
- [ ] Fonctions.

(Ordre et contenu à ajuster au fur et à mesure, en fonction de ce qu'on a envie de comprendre ensuite.)

## Phase 3 — Windows (self-contained, cross-compilé depuis Linux)

- [ ] `dotnet publish -r win-x64 --self-contained` depuis la machine Linux.
- [ ] Vérification du binaire Windows (VM ou CI avec runner Windows — impossible à exécuter depuis Linux directement).
- [ ] Documenter les différences éventuelles (chemins, fins de ligne, encodage console, etc.).

## Phase 4 — Génération de code natif (sans Roslyn)

Objectif : remplacer l'étape "transpile en C# + Roslyn" par un vrai générateur de code machine, pour aller jusqu'au bout de l'exercice.

- [x] Approche retenue : émettre de l'assembleur x86-64 (texte), déléguer l'assemblage et l'édition de liens à des outils existants (`as`/`ld` sur Linux, équivalent mingw pour Windows). On écrit vraiment la génération de code (instructions, registres, pile, syscalls) sans réinventer le format ELF/PE.
- [ ] Gestion des conventions d'appel, de la pile, des syscalls (Linux) / API Windows pour les I/O (`print`).
- [ ] Réutiliser les mêmes tests bout en bout que la phase 1 (même langage, nouveau backend) pour vérifier la non-régression.

## Phase 5 — Exploration Rust (plus tard)

- [ ] Réécriture du compilateur en Rust une fois les bases bien comprises en C#.
- [ ] Comparaison des deux implémentations à but pédagogique.

## Backlog / idées non priorisées

- Native AOT (compilation du C# généré par la phase 1 vers du natif) si on veut un binaire natif rapide sans attendre la phase 4 — nécessite de builder sur l'OS cible, donc pas de cross-compilation possible. Différent de la phase 4 : ici Roslyn/le runtime .NET fait toujours la génération de code, on ne fait qu'éviter le JIT.
- REPL.
