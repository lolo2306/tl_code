# TL

Compilateur d'un langage simple, écrit pour comprendre comment fonctionne un compilateur — pas pour produire un langage de production.

Voir [docs/ROADMAP.md](docs/ROADMAP.md) pour les étapes et [docs/language-spec.md](docs/language-spec.md) pour ce que le langage sait faire aujourd'hui.

## Prérequis

- .NET SDK 10.0+

## Build & tests

```bash
dotnet build
dotnet test
```

## Compiler un fichier `.tl`

```bash
dotnet run --project src/Tl.Compiler -- chemin/vers/fichier.tl -o mon_programme
./mon_programme
```

Le binaire produit est autonome : pas besoin d'installer .NET pour l'exécuter (le runtime est embarqué).
