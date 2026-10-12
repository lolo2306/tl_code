# Spécification du langage TL

## v4 (phase 2) — structures de contrôle

### Grammaire

```
programme    := instruction*
instruction  := printInstruction | déclarationVariable | assignation | ifInstruction | whileInstruction
printInstruction    := "print" expression ";"
déclarationVariable := "var" identifiant "=" expression ";"
assignation         := identifiant "=" expression ";"
ifInstruction        := "if" "(" expression ")" bloc ( "else" (ifInstruction | bloc) )?
whileInstruction      := "while" "(" expression ")" bloc
bloc                 := "{" instruction* "}"

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

- Déclaration : `var nom = valeur;`.
- Réassignation (sans `var`) : `nom = valeur;` — la variable doit déjà être déclarée et la nouvelle valeur doit être du même type.
- Utiliser une variable non déclarée est une erreur de compilation.

### Portée des variables (scoping)

- Chaque bloc `{ ... }` (corps de `if`/`else`/`while`) introduit sa propre portée : une variable déclarée dedans n'existe plus une fois le bloc terminé.
- **Pas de masquage (shadowing)** : on ne peut pas redéclarer un nom déjà utilisé dans une portée englobante encore active, même dans un bloc imbriqué. Choix volontaire pour rester simple — pas besoin de réfléchir à "quel `x` est utilisé ici ?".
- Un bloc peut en revanche réutiliser un nom déclaré dans un bloc *frère* (non imbriqué), puisque la portée du premier est refermée avant que le second ne commence.
- Une variable déclarée dans une portée englobante reste assignable depuis un bloc imbriqué (c'est ce qui permet de faire varier la condition d'un `while`).

### Structures de contrôle

- `if (condition) { ... }`, avec `else { ... }` optionnel, et `else if (...) { ... }` en chaîne.
- `while (condition) { ... }`.
- La condition doit être de type `bool` (vérifié à la compilation).
- Les accolades sont **obligatoires** : pas de raccourci type `if (cond) print x;` sans bloc (choix volontaire pour rester simple et éviter toute ambiguïté).

### Chaînes de caractères

Séquences d'échappement supportées : `\"`, `\\`, `\n`, `\t`.

### Messages d'erreur

Toute erreur de compilation (lexicale ou syntaxique) indique la ligne et la colonne où elle a été détectée, ex : `Variable 'y' non déclarée (ligne 2, colonne 7)`.

### Exemple

```tl
var i = 0;
while (i < 5) {
    print i;
    i = i + 1;
}

var age = 42;
if (age < 18) {
    print "mineur";
} else if (age < 65) {
    print "actif";
} else {
    print "retraite";
}
```

### Non supporté (volontairement, pour l'instant)

- Concaténation de chaînes avec `+`.
- Fonctions.
- Commentaires.
- `for`, `break`, `continue`.

Voir [ROADMAP.md](ROADMAP.md) pour les phases suivantes.
