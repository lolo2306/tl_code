# Spécification du langage TL

## v3 (phase 2) — expressions

### Grammaire

```
programme    := instruction*
instruction  := printInstruction | déclarationVariable
printInstruction    := "print" expression ";"
déclarationVariable := "var" identifiant "=" expression ";"

expression   := ou
ou           := et ("||" et)*
et           := égalité ("&&" égalité)*
égalité      := relation (("==" | "!=") relation)*
relation     := additive (("<" | ">" | "<=" | ">=") additive)*
additive     := multiplicative (("+" | "-") multiplicative)*
multiplicative := unaire (("*" | "/" | "%") unaire)*
unaire       := ("-" | "!") unaire | primaire
primaire     := chaîne | entier | booléen | identifiant | "(" expression ")"

chaîne       := '"' caractère* '"'
entier       := chiffre+
booléen      := "true" | "false"
```

### Opérateurs

| Opérateur(s)            | Opérandes       | Résultat | Remarque |
|--------------------------|-----------------|----------|----------|
| `+ - * / %`              | `int`, `int`    | `int`    | division entière (comme en C#) |
| `< > <= >=`              | `int`, `int`    | `bool`   | |
| `== !=`                  | même type des deux côtés | `bool` | string/int/bool |
| `&& \|\|`                | `bool`, `bool`  | `bool`   | |
| `-` (unaire)             | `int`           | `int`    | |
| `!` (unaire)             | `bool`          | `bool`   | |
| `( expression )`         | —               | type de l'expression | pour forcer la précédence |

Mélanger les types (ex : `"a" + 1`, `1 && 2`) est une erreur de compilation, détectée avec ligne/colonne.

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
print "Bonjour !";
print name;
print age >= 18 && age < 65;
print (2 + 3) * 4;
print age % 10;
```

### Non supporté (volontairement, pour l'instant)

- Réassignation de variables.
- Concaténation de chaînes avec `+`.
- Structures de contrôle (if/while).
- Fonctions.
- Commentaires.

Voir [ROADMAP.md](ROADMAP.md) pour les phases suivantes.
