# Conventions de tests

## Framework

- xUnit (standard .NET, intégration simple avec `dotnet test`).
- Un projet de tests par projet source (ex: `Tl.Compiler.Tests` pour `Tl.Compiler`).

## Niveaux de tests

- **Unitaires** : lexer (un token attendu par entrée), parser (AST attendu par suite de tokens), génération de code — testés indépendamment, sans passer par tout le pipeline.
- **Bout en bout** : fichiers `.tl` de test dans `tests/fixtures/`, compilés puis exécutés, sortie stdout comparée à un fichier attendu ("golden file"). Un cas par fonctionnalité du langage.

## Mocks

- Le compilateur n'a quasiment pas de dépendances externes (pas de réseau, pas de base de données) : les mocks ne devraient pas être nécessaires pour le cœur du pipeline (lexer/parser/codegen).
- Si besoin d'isoler le système de fichiers (ex: tester le driver CLI sans écrire de vrais fichiers), passer par une abstraction (`IFileSystem` ou équivalent) et `NSubstitute` ou `Moq` pour la simuler.

## Règles

- Un bug corrigé = un test de non-régression qui le couvre.
- Ne pas tester des détails d'implémentation qui changeront souvent (ex: ne pas vérifier le C# généré caractère pour caractère si un test bout-en-bout sur le comportement observable suffit).
