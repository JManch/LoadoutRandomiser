# Loadout Randomiser

LoadoutRandomiser is a simple console application for generating random game loadouts. Here is an example output from generating a Battlefield V loadout:
```
Class:  Assault             
Combat Role:  Vehicle Buster
Primary:  Sturmgewehr 1-5   
Sights:  3x                 
Sidearm:  P38 Pistol        
Melee:  Kukri               
Grenade:  Demolition Grenade
Gadget 1:  M1A1 Bazooka     
Gadget 2:  Lunge Mine       
```

**Currently Supported Games**

- Battlefield V
- Deep Rock Galactic

## Adding a Game

To add a game create a .loadout file in the LoadoutData directory. Loadouts are defined by "categories" and "options". 

| Syntax      | Description |
| :----: | ----------- |
| :           | Defines a category       |
| ,           | Defines an option        |
| *           | Defines an option select count        |
| { }           | Defines a child to a category or option        |

### Basic category example

```
CategoryName:
{
    Option1,
    Option2,
    Option3,
}
```
Will produce the following output:

```
> generate Example    
                      
CategoryName:  Option2

> _
```
As you can see, one of the options inside the category was randomly chosen.

**Conditional category example**

If a category is a child of a point, it will only be rolled if the parent option is randomly selected. 
```
CategoryName:
{
    Option1,
    {
        ConditionalCategory:
        {
            ConditionalOption1,
            ConditionalOption2,
        }
    }
    Option2,
    Option3,
}
```
Will produce the following two outputs when Option1 is selected and Option2 is selected:

*Option1 selected*
```
> generate Example    
                      
CategoryName:  Option1
ConditionalCategory:  ConditionalOption2

> _
```
*Option2 selected*
```
> generate Example    
                      
CategoryName:  Option2

> _
```
**Randomly selecting multiple options**

Multiple options can be randomly selected from a list of options the following way:
```
CategoryName: 3*
{
    Option1,
    Option2,
    Option3,
    Option4,
}
```
The 3* means that 3 unique options will be selected. Note that the number of options must exceed the option select count.

Here is the output:
```
> generate Example    
                      
CategoryName:  Option1, Option3, Option2

> _
```
