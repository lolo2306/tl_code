# Référence du langage TL

Documentation de référence : chaque instruction du langage, sa syntaxe, ses paramètres et des exemples. La grammaire complète (BNF) est en annexe, en bas de ce document.

## Sommaire

- [Types](#types)
- [Instructions](#instructions)
  - [`print`](#print)
  - [`var` — déclaration de variable](#var--déclaration-de-variable)
  - [Assignation](#assignation)
  - [`if` / `else`](#if--else)
  - [`while`](#while)
  - [`fn` — déclaration de fonction](#fn--déclaration-de-fonction)
  - [`return`](#return)
  - [Appel de fonction](#appel-de-fonction)
- [Expressions et opérateurs](#expressions-et-opérateurs)
- [Portée des variables (scoping)](#portée-des-variables-scoping)
- [Messages d'erreur](#messages-derreur)
- [Non supporté (volontairement, pour l'instant)](#non-supporté-volontairement-pour-linstant)
- [Annexe : grammaire complète](#annexe--grammaire-complète)

## Types

Trois types. Pour une variable, le type est **inféré** depuis la valeur assignée (pas d'annotation). Pour un paramètre de fonction, le type est **annoté explicitement** (seul endroit du langage où c'est le cas).

| Type TL  | Exemple de littéral |
|----------|----------------------|
| `string` | `"hello"`            |
| `int`    | `42`                 |
| `bool`   | `true` / `false`     |

Séquences d'échappement supportées dans une chaîne : `\"`, `\\`, `\n`, `\t`.

## Instructions

### `print`

Affiche une valeur sur la sortie standard, suivie d'un retour à la ligne.

**Syntaxe :** `print <expression>;`

**Paramètres :**
| Paramètre | Type | Description |
|---|---|---|
| `expression` | `string`, `int` ou `bool` | La valeur à afficher. Ne peut pas être le résultat d'un appel void. |

**Exemples :**
```tl
print "Hello, world!";
print 42;
print age >= 18;
```

### `var` — déclaration de variable

Déclare une nouvelle variable, typée par inférence depuis sa valeur initiale.

**Syntaxe :** `var <nom> = <expression>;`

**Paramètres :**
| Paramètre | Type | Description |
|---|---|---|
| `nom` | identifiant | Nom de la variable. Doit être unique dans toutes les portées actives (pas de masquage, voir [scoping](#portée-des-variables-scoping)). |
| `expression` | `string`, `int` ou `bool` | Valeur initiale ; détermine le type de la variable. Ne peut pas être le résultat d'un appel void. |

**Exemple :**
```tl
var name = "Laurent";
var age = 42;
var isDev = true;
```

### Assignation

Change la valeur d'une variable déjà déclarée. La nouvelle valeur doit être du même type que la valeur d'origine.

**Syntaxe :** `<nom> = <expression>;`

**Paramètres :**
| Paramètre | Type | Description |
|---|---|---|
| `nom` | identifiant | Nom d'une variable déjà déclarée et visible dans la portée courante. |
| `expression` | même type que la variable | Nouvelle valeur. |

**Exemple :**
```tl
var i = 0;
i = i + 1;
```

### `if` / `else`

Exécute un bloc selon une condition booléenne, avec branche alternative optionnelle.

**Syntaxe :**
```
if (<condition>) { ... }
if (<condition>) { ... } else { ... }
if (<condition>) { ... } else if (<condition>) { ... } else { ... }
```

**Paramètres :**
| Paramètre | Type | Description |
|---|---|---|
| `condition` | `bool` | Évaluée pour choisir la branche. Toute autre type est une erreur de compilation. |

Les accolades sont **obligatoires** (pas de raccourci `if (cond) print x;` sans bloc). `else if` n'est pas une règle à part : c'est un `if` imbriqué dans le bloc `else`.

**Exemple :**
```tl
if (age < 18) {
    print "mineur";
} else if (age < 65) {
    print "actif";
} else {
    print "retraite";
}
```

### `while`

Répète un bloc tant qu'une condition booléenne est vraie.

**Syntaxe :** `while (<condition>) { ... }`

**Paramètres :**
| Paramètre | Type | Description |
|---|---|---|
| `condition` | `bool` | Réévaluée avant chaque itération. |

**Exemple :**
```tl
var i = 0;
while (i < 5) {
    print i;
    i = i + 1;
}
```

### `fn` — déclaration de fonction

Déclare une fonction : un ensemble de paramètres typés, un corps, et un type de retour optionnel.

**Syntaxe :**
```
fn <nom>(<param>: <type>, ...) -> <type> { ... }   // fonction qui retourne une valeur
fn <nom>(<param>: <type>, ...) { ... }              // fonction void (ne retourne rien)
```

**Paramètres :**
| Paramètre | Type | Description |
|---|---|---|
| `nom` | identifiant | Nom de la fonction. Doit être unique (pas de surcharge). |
| `param: type` | `int`, `string` ou `bool` | Paramètre nommé et typé explicitement. Les noms de paramètres doivent être uniques entre eux. |
| `-> type` | `int`, `string` ou `bool` | Type de retour. Omis = la fonction ne retourne rien (void). |

**Règles importantes :**
- **Portée isolée** : le corps de la fonction ne voit *que* ses paramètres et ses variables locales — aucun accès aux variables globales ou extérieures (pas de closures).
- **Récursion directe** possible (une fonction peut s'appeler elle-même). **Pas de récursion mutuelle** : comme pour les variables, une fonction doit être déclarée avant d'être appelée ailleurs.
- Le compilateur ne vérifie pas que toutes les branches d'une fonction typée se terminent par un `return` — un oubli sera détecté par le compilateur C# sous-jacent (message moins clair, sans ligne/colonne TL).

**Exemple :**
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
```

### `return`

Termine l'exécution de la fonction courante, avec ou sans valeur.

**Syntaxe :**
```
return <expression>;   // dans une fonction typée
return;                // dans une fonction void
```

**Paramètres :**
| Paramètre | Type | Description |
|---|---|---|
| `expression` | doit correspondre au type de retour déclaré de la fonction | Valeur retournée. Absente dans une fonction void. |

Utiliser `return` en dehors d'une fonction est une erreur de compilation.

### Appel de fonction

Appelle une fonction déclarée, avec ses arguments.

**Syntaxe :** `<nom>(<argument>, ...)`

**Paramètres :**
| Paramètre | Type | Description |
|---|---|---|
| `nom` | identifiant d'une fonction déjà déclarée | |
| `argument` | doit correspondre terme à terme aux types des paramètres déclarés | Le nombre d'arguments doit correspondre exactement. |

Un appel peut être utilisé :
- **comme expression**, s'il retourne une valeur : `print add(1, 2);`, `var x = add(1, 2);`
- **comme instruction à lui seul** : `greet();` (fonctionne aussi pour une fonction qui retourne une valeur — celle-ci est alors ignorée)

Appeler une fonction void là où une valeur est attendue (`print f();`, `var x = f();`) est une erreur de compilation.

**Exemple :**
```tl
print factorial(5);
greet("Laurent");
```

## Expressions et opérateurs

| Opérateur(s)       | Opérandes                | Résultat | Remarque |
|--------------------|---------------------------|----------|----------|
| `+ - * / %`        | `int`, `int`               | `int`    | division entière (comme en C#) |
| `< > <= >=`        | `int`, `int`               | `bool`   | |
| `== !=`            | même type des deux côtés   | `bool`   | string/int/bool |
| `&& \|\|`          | `bool`, `bool`             | `bool`   | |
| `-` (unaire)       | `int`                      | `int`    | |
| `!` (unaire)       | `bool`                     | `bool`   | |
| `( expression )`   | —                          | type de l'expression | pour forcer la précédence |

Précédence (du plus faible au plus fort) : `||` → `&&` → `==`/`!=` → `<`/`>`/`<=`/`>=` → `+`/`-` → `*`/`/`/`%` → unaire (`-`, `!`) → primaire (littéraux, variables, appels, parenthèses).

Mélanger les types (ex : `"a" + 1`, `1 && 2`) est une erreur de compilation, détectée avec ligne/colonne.

## Portée des variables (scoping)

- Chaque bloc `{ ... }` (corps de `if`/`else`/`while`) introduit sa propre portée : une variable déclarée dedans n'existe plus une fois le bloc terminé.
- **Pas de masquage (shadowing)** : impossible de redéclarer un nom déjà utilisé dans une portée englobante encore active, même dans un bloc imbriqué. Choix volontaire pour rester simple.
- Deux blocs *frères* (non imbriqués) peuvent réutiliser le même nom, puisque la portée du premier est refermée avant que le second ne commence.
- Une variable déclarée dans une portée englobante reste assignable depuis un bloc imbriqué (c'est ce qui permet de faire varier la condition d'un `while`).
- Une fonction a sa propre portée isolée : voir [`fn`](#fn--déclaration-de-fonction).

## Messages d'erreur

Toute erreur de compilation (lexicale ou syntaxique) indique la ligne et la colonne où elle a été détectée, ex :

```
Variable 'y' non déclarée (ligne 2, colonne 7)
```

## Non supporté (volontairement, pour l'instant)

- Concaténation de chaînes avec `+`.
- Récursion mutuelle (fonctions déclarées dans le désordre qui s'appellent l'une l'autre).
- Fonctions imbriquées, closures, fonctions comme valeurs.
- Surcharge de fonctions.
- Commentaires.
- `for`, `break`, `continue`.

Voir [ROADMAP.md](ROADMAP.md) pour les phases suivantes.

## Annexe : grammaire complète

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
