# Spécification du langage TL

## v2 (phase 2) — variables et types

### Grammaire

```
programme    := instruction*
instruction  := printInstruction | déclarationVariable
printInstruction    := "print" valeur ";"
déclarationVariable := "var" identifiant "=" valeur ";"
valeur       := chaîne | entier | booléen | identifiant
chaîne       := '"' caractère* '"'
entier       := chiffre+
booléen      := "true" | "false"
```

### Types

Trois types, inférés depuis la valeur assignée (pas d'annotation de type explicite) :

| Type TL  | Exemple de littéral |
|----------|----------------------|
| `string` | `"hello"`            |
| `int`    | `42`                 |
| `bool`   | `true` / `false`     |

### Variables

- Déclaration unique : `var nom = valeur;`. Redéclarer un nom déjà utilisé est une erreur de compilation.
- Utiliser une variable non déclarée (dans `print` ou dans une autre déclaration) est une erreur de compilation.
- Pas de réassignation pour l'instant (`x = nouvelle_valeur;` sans `var` n'existe pas encore).

### Chaînes de caractères

Séquences d'échappement supportées : `\"`, `\\`, `\n`, `\t`.

### Messages d'erreur

Toute erreur de compilation (lexicale ou syntaxique) indique la ligne et la colonne où elle a été détectée, ex : `Variable 'y' non déclarée (ligne 2, colonne 7)`.

### Exemple

```tl
var name = "Laurent";
var age = 42;
var isDev = true;
print "Bonjour !";
print name;
print age;
print isDev;
```

### Non supporté (volontairement, pour l'instant)

- Expressions (opérateurs arithmétiques/logiques) — prochaine étape de la phase 2.
- Réassignation de variables.
- Structures de contrôle (if/while).
- Fonctions.
- Commentaires.

Voir [ROADMAP.md](ROADMAP.md) pour les phases suivantes.
