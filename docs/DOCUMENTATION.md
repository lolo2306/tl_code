# Conventions de documentation

## Emplacement

- `docs/` pour tout ce qui n'est pas du code : roadmap, spec du langage, conventions.
- `docs/language-spec.md` : à créer au fur et à mesure qu'on ajoute des instructions au langage (syntaxe + sémantique de chaque feature). Pas encore créé — à faire dès la phase 1.
- `README.md` à la racine : quickstart (builder le compilateur, compiler un `.tl`, l'exécuter). Pas encore créé.

## Code

- Pas de commentaires par défaut ; uniquement pour expliquer un "pourquoi" non évident (contournement, contrainte cachée, invariant surprenant).
- Pas de doc XML (`///`) systématique sur chaque méthode — seulement sur l'API publique si le projet grossit et qu'elle est consommée ailleurs.

## Langue

- Français pour la doc de collaboration (roadmap, ce fichier, commentaires de conception) — à réévaluer si le projet devient public / open source, auquel cas basculer en anglais.
