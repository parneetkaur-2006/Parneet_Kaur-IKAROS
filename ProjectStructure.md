# Ikaros: Project Structure

```
Assets/
│
├── Animations/                 # Animation controllers and clips
│   ├── Characters/             
│   ├── Enemies/                
│   ├── UI/                     
│   └── Environment/            
│
├── Audio/                      # Sound effects and music
│   ├── Music/                  # Background music for each land
│   ├── SFX/                    # Sound effects
│   └── Voice/                  # Character dialogues
│
├── Materials/                  # Materials for objects
│
├── Models/                     # 3D models if needed for 2.5D elements
│
├── Prefabs/                    # Reusable game objects
│   ├── Characters/             
│   ├── Enemies/                
│   ├── Weapons/                
│   ├── Environments/           
│   └── UI/                     
│
├── Resources/                  # Assets loaded at runtime
│
├── Scenes/                     
│   ├── MainMenu/               
│   ├── Hellas/                 # Greece world
│   ├── Hispania/               # Spain world
│   ├── Pindorama/              # Brazil world
│   ├── Kemet/                  # Egypt world
│   ├── Jambudweep/             # India world
│   ├── Zhongguo/               # China world
│   ├── Wano/                   # Japan world
│   └── Kruvija/                # Final boss area
│
├── Scripts/                    # C# scripts
│   ├── Core/                   # Core game systems
│   │   ├── SaveSystem/         # Save/load functionality
│   │   └── GameManager/        # Overall game state management
│   │
│   ├── Characters/
│   │   ├── Player/             # Player character scripts
│   │   ├── NPCs/               # Non-playable characters
│   │   ├── Enemies/            # Enemy behaviors
│   │   └── Commanders/         # Boss behaviors
│   │
│   ├── Combat/                 # Combat systems
│   │   ├── Weapons/
│   │   ├── Abilities/
│   │   └── Effects/
│   │
│   ├── Environment/            # Environment interactions
│   │
│   ├── Items/                  # Collectibles and items
│   │   ├── Weapons/
│   │   └── Cuisines/           # Food items with effects
│   │
│   ├── UI/                     # User interface
│   │   ├── Menus/
│   │   ├── HUD/
│   │   └── Accessibility/      # Accessibility features
│   │
│   ├── Festival/               # Festival revival system
│   │
│   └── Utils/                  # Utility scripts
│
├── Shaders/                    # Custom shaders
│
├── Sprites/                    # 2D artwork
│   ├── Characters/
│   ├── UI/
│   ├── Environments/           # Background art for each world
│   ├── Items/
│   └── Effects/
│
└── StreamingAssets/            # Assets streamed from disk
``` 
