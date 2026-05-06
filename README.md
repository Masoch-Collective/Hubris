# Hubris

Hubris (f.k.a. Tempus Fugit) is a 1v1 platform fighting game by Masoch Collective. It is being developed as a capstone project for Sheridan College's OCGC program, Game Development — Advanced Programming.

## Management

Progress for this project is being tracked via [HackNPlan](https://app.hacknplan.com/p/235229/gamemodel). 

## Members

- [Jayde Iris Callejas](https://callejas.xyz/) — Programming, Game Design
- [Mian (Matt) Si](https://github.com/MattSi) — Programming
- [Evan Caie](https://evancaiegames.com) — Game Design
- [Maria Salas Duran](https://sites.google.com/view/fersalasportfolio/home) — Art

## Technical Specs

Developed with Unity 6000.3.6f1 LTS

## Rules, Guidelines & Best Practices

<details>
<summary>Code & Style</summary>

- Please follow variable naming conventions and be mindful of accessibility level
  - Fields should be private and use the `[SerializeField]` attribute if inspector access is intended
  - If read-only access is required from other classes, use public-get/private-set properties
    - Properties with default accessors can be serialized in the Unity inspector with the attribute `[field:SerializeField]`
  - [JetBrains Rider](https://www.jetbrains.com/rider/) recommended (free for students)—it throws warnings when a variable name does not follow conventions, and tells you how to name them accordingly
- Place classes in the appropriate namespace
  - Namespace and folder structure should match, (not including `//Assets/Core/Scripts`) e.g. the script `//Assets/Core/Scripts/PlayerController/Movement/AerialController.cs` should contain a class named `AerialController` in the namespace `PlayerController.Movement`

</details>

<details> 
<summary>Commits</summary>

- Commits should not be made directly to main branch
- Commit messages should be detailed and must properly explain what changes were made and why
- Commits should be frequent and concise—avoid making several disparate changes in a single commit
- Commits should only include necessary changes (i.e. avoid committing minute changes such as line endings, adjustments to spacing/formatting, etc.)

</details>

<details>
<summary>Merging</summary>

- Code being merged into main should not produce errors or warnings
- Merges should be made via pull requests, and should be reviewed by others before being merged into main

</details>

<details>
<summary>Miscellaneous/Unity-specific</summary>

- Each contributor should have their own individual test scene for development/testing to avoid merge conflicts
- Modifications to game scenes should be made with care and discussed to avoid merge conflicts
- Modifications to others' prefabs should be discussed and communicated to avoid merge conflicts

</details>
