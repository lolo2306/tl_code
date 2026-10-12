# Spécification du langage TL

## v5 (phase 2) — fonctions

### Grammaire

```
programme    := (déclarationFonction | instruction)*

déclarationFonction := "fn" identifiant "(" paramètres? ")" ("->" type)? bloc
paramètres   := paramètre ("," paramètre)*
paramètre    := identifiant ":" type
type         := "int" | "string" | "bool"

instruction  := printInstruction | déclarationVariable | assignation | ifInstruction
              | whileInstruction | retour | appelInstruction
printInstruction    := "print" expression ";"
déclarationVariable := "var" identifiant "=" expression ";"
assignation         := identifiant "=" expression ";"
ifInstruction        := "if" "(" expression ")" bloc ( "else" (ifInstruction | bloc) )?
whileInstruction      := "while" "(" expression ")" bloc
retour               := "return" expression? ";"
appelInstruction     := appel ";"
bloc                 := "{" instruction* "}"

expression   := ou
ou           := et ("||" et)*
et           := égalité ("&&" égalité)*
égalité      := relation (("==" | "!=") relation)*
relation     := additive (("<" | ">" | "<=" | ">=") additive)*
additive     := multiplicative (("+" | "-") multiplicative)*
multiplicative := unaire (("*" | "/" | "%") unaire)*
unaire       := ("-" | "!") unaire | primaire
primaire     := chaîne | entier | booléen | identifiant | appel | "(" expression ")"
appel        := identifiant "(" arguments? ")"
arguments    := expression ("," expression)*

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

### Fonctions

- Déclaration : `fn nom(param: type, ...) -> type { ... }`. Le `-> type` est omis pour une fonction qui ne retourne rien (void).
- Les paramètres ont un type annoté explicitement (`int`, `string` ou `bool`) — c'est le seul endroit du langage où le type ne s'infère pas.
- `return expression;` dans une fonction typée, `return;` (sans expression) ou rien dans une fonction void. Le type de l'expression retournée doit correspondre au type de retour déclaré.
- **Portée isolée** : une fonction ne voit *que* ses paramètres et ses variables locales (déclarées dans son propre corps). Elle n'a accès à aucune variable globale ou extérieure — pas de closures, pas d'état partagé implicite.
- **Récursion directe** supportée (une fonction peut s'appeler elle-même : sa signature est enregistrée avant que son corps ne soit analysé). **Pas de récursion mutuelle** : une fonction doit être déclarée avant d'être appelée par une autre (même règle que les variables), donc deux fonctions ne peuvent pas s'appeler l'une l'autre pour l'instant.
- Un appel peut être utilisé comme expression (`print add(1, 2);`) ou comme instruction à lui seul (`greet();`) s'il ne retourne rien — ou même s'il retourne quelque chose, auquel cas la valeur est simplement ignorée.
- Appeler une fonction qui ne retourne rien (void) dans un contexte qui attend une valeur (`print f();`, `var x = f();`, dans une expression) est une erreur de compilation.
- Pas de surcharge (deux fonctions ne peuvent pas partager le même nom, quels que soient leurs paramètres).

**Limitation connue, assumée pour rester simple :** le compilateur ne vérifie pas que toutes les branches d'une fonction typée se terminent par un `return`. Un oubli sera détecté par le compilateur C# sous-jacent lors du `dotnet publish`, avec un message moins clair que nos erreurs habituelles (pas de ligne/colonne TL).

### Chaînes de caractères

Séquences d'échappement supportées : `\"`, `\\`, `\n`, `\t`.

### Messages d'erreur

Toute erreur de compilation (lexicale ou syntaxique) indique la ligne et la colonne où elle a été détectée, ex : `Variable 'y' non déclarée (ligne 2, colonne 7)`.

### Exemple

```tl
fn factorial(n: int) -> int {
    if (n == 0) {
        return 1;
    }
    return n * factorial(n - 1);
}

fn greet(name: string) {
    print "Bonjour, ";
    print name;
}

print factorial(5);
greet("Laurent");
```

### Non supporté (volontairement, pour l'instant)

- Concaténation de chaînes avec `+`.
- Récursion mutuelle (fonctions déclarées dans le désordre qui s'appellent l'une l'autre).
- Fonctions imbriquées, closures, fonctions comme valeurs.
- Surcharge de fonctions.
- Commentaires.
- `for`, `break`, `continue`.

Voir [ROADMAP.md](ROADMAP.md) pour les phases suivantes.
