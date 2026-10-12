# Spécification du langage TL

## v1 (phase 1) — `print`

### Grammaire

```
programme  := instruction*
instruction := "print" chaîne ";"
chaîne      := '"' caractère* '"'
```

### Chaînes de caractères

Délimitées par des guillemets doubles. Séquences d'échappement supportées :

| Séquence | Signification      |
|----------|---------------------|
| `\"`     | guillemet double     |
| `\\`     | antislash             |
| `\n`     | retour à la ligne     |
| `\t`     | tabulation            |

### Exemple

```tl
print "Hello, world!";
print "Ligne 1\nLigne 2";
```

### Non supporté (volontairement, pour l'instant)

- Variables, expressions, types.
- Structures de contrôle (if/while).
- Fonctions.
- Commentaires.

Voir [ROADMAP.md](ROADMAP.md) pour les phases suivantes.
