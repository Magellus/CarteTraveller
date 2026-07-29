using CarteTraveller.Models;

namespace CarteTraveller.Services
{
    public class SectorGeneratorService
    {
        // Génère un secteur complet
        public Sector GenerateSector(string sectorName, int densityModifier = 0)
        {
            var sector = new Sector { Name = sectorName };

            // Parcours de la grille 32 x 40
            for (int x = 1; x <= 32; x++)
            {
                for (int y = 1; y <= 40; y++)
                {
                    // Vérifie si un système stellaire est présent dans cet hexagone
                    // Standard Traveller: Sur 1D6, on place un monde sur un 4, 5 ou 6.
                    if (System.Random.Shared.Next(1, 7) + densityModifier >= 4)
                    {
                        var world = GenerateWorld(sectorName, x, y);
                        sector.Worlds.Add(world.HexCoordinate, world);
                    }
                }
            }

            return sector;
        }

        private World GenerateWorld(string name,int x, int y)
        {
            var world = new World
            {
                HexX = x,
                HexY = y,
                Name = name + $": {x:D2}{y:D2}"
            };

            world.Profile = GenerateUwp();

            // TODO: Générer les bases (Naval, Scout), allégeance, géantes gazeuses, etc.

            return world;
        }

        private Uwp GenerateUwp()
        {
            var uwp = new Uwp();

            // 1. Size (2D6 - 2)
            uwp.Size = Dice.Roll(2) - 2;

            if(uwp.Size <= 1)
            {
                uwp.Atmosphere = 0; // Atmosphere is 0 for worlds with size less than 2
                uwp.Hydrographics = 0; // Hydrographics is 0 for worlds with size less than 2
            }
            else
            {
                // 2. Atmosphere (2D6 - 7 + Size)
                uwp.Atmosphere = Math.Max(0, Dice.Roll(2) - 7 + uwp.Size);

                // 3. Hydrographics
                if (uwp.Atmosphere <= 1 || uwp.Atmosphere >= 10)
                {
                    uwp.Hydrographics = Math.Max(0, Dice.Roll(2) - 11 + uwp.Atmosphere);
                }
                else
                {
                    uwp.Hydrographics = Math.Max(0, Dice.Roll(2) - 7 + uwp.Atmosphere);
                }
                
            }

            uwp.Temperature = Dice.Roll(2);
            // TODO: ajouter logique de sans atmosphère. roasting le jour et freezing la nuit.
            if (uwp.Atmosphere == 0 || uwp.Atmosphere == 0) {}
            else if (uwp.Atmosphere <= 3) { uwp.Temperature -= 2; } 
            else if (uwp.Atmosphere <= 5 || uwp.Atmosphere == 14) { uwp.Temperature -= 1; }
            else if (uwp.Atmosphere <= 7) {} 
            else if (uwp.Atmosphere <= 9) { uwp.Temperature += 1; } 
            else if (uwp.Atmosphere == 10 || uwp.Atmosphere == 13 || uwp.Atmosphere == 15) { uwp.Temperature += 2; } 
            else if (uwp.Atmosphere == 11 || uwp.Atmosphere == 12) { uwp.Temperature += 6; } 

            // 4. Population
            uwp.Population = Math.Max(0, Dice.Roll(2) - 2);

            // 5. Government
            // 6. Law Level
            // 7. Starport
            // 8. Tech Level
            if (uwp.Population == 0)
            {
                uwp.Government = 0;
                uwp.LawLevel = 0;
                uwp.TechLevel = 0; 
                uwp.Starport = 'X'; // No starport for uninhabited worlds // TODO: en fait, il pourrait y avoir un starport pour un monde inhabité, mais c'est rare. À voir si on veut le gérer.
            }
            else
            {
                uwp.Government = Math.Max(0, Dice.Roll(2) - 7 + uwp.Population);
                // TODO: ajouter code pour faction rivale et modifier le gouvernement en conséquence
                uwp.LawLevel = Math.Max(0, Dice.Roll(2) - 7 + uwp.Government);

                int populationModifier = 0;
                if (uwp.Population <= 2)
                {
                    populationModifier = -2;
                }
                else if (uwp.Population <= 4)
                {
                    populationModifier = -1;
                }
                else if (uwp.Population <= 7)
                {
                    populationModifier = 0;
                }
                else if (uwp.Population <= 9)
                {
                    populationModifier = 1;
                }
                else
                {
                    populationModifier = 2;
                }

                    int starportRoll = Dice.Roll(2) + populationModifier;


                if (starportRoll <= 2) { uwp.Starport = 'X'; } 
                else if (starportRoll <= 4) { uwp.Starport = 'E'; } 
                else if (starportRoll <= 6) { uwp.Starport = 'D'; } 
                else if (starportRoll <= 8) { uwp.Starport = 'C'; } 
                else if (starportRoll <= 10) { uwp.Starport = 'B'; } 
                else { uwp.Starport = 'A'; }

            }

            uwp.TechLevel = CalculateTechLevel(uwp);


            return uwp;
        }

        public int CalculateTechLevel(Uwp uwp)
        {
            int techLevel = Dice.Roll(1);
            // Base Tech Level based on Starport
            switch (uwp.Starport)
            {
                case 'A':
                    techLevel += 6;
                    break;
                case 'B':
                    techLevel += 4;
                    break;
                case 'C':
                    techLevel += 2;
                    break;
                case 'X':
                    techLevel -= 4;
                    break;
            }
            // Adjustments based on other UWP characteristics
            if (uwp.Size <= 4) techLevel += 1; 
            if (uwp.Size <= 1) techLevel += 1; 
            if (uwp.Atmosphere <= 3 || uwp.Atmosphere >= 10) techLevel += 1; 
            if (uwp.Hydrographics == 0 || uwp.Hydrographics == 9) techLevel += 1;
            if (uwp.Hydrographics == 10) techLevel += 2;
            if (uwp.Population > 0 && uwp.Population < 6) techLevel += 1;
            if (uwp.Population == 8) techLevel += 1;
            if (uwp.Population == 9) techLevel += 4;
            if (uwp.Population == 10) techLevel += 4;
            if (uwp.Government == 0 || uwp.Government == 5) techLevel += 1;
            if (uwp.Government == 7) techLevel += 2;
            if (uwp.Government == 13 || uwp.Government == 14) techLevel -= 2;
            return Math.Max(0, techLevel); // Ensure Tech Level is not negative
        }
    }
}
