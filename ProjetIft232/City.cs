﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using ProjetIft232.Army;
using ProjetIft232.Buildings;
using ProjetIft232.Technologies;
using System.Collections.ObjectModel;

namespace ProjetIft232
{
    public class City
    { 
        public static Resources CostToCreate = new Resources(500, 500, 500, 500, 500);
        private Resources BaseProduction()
        {
            return new Resources(new Dictionary<ResourcesType, int>() {
            {ResourcesType.Wood, 5},
            {ResourcesType.Gold, 5},
            {ResourcesType.Meat, 5},
            {ResourcesType.Rock, 5},
            {ResourcesType.Population, Convert.ToInt32(1 + _tourDepuisCreation * 0.1)}
        });
        }
        public ObservableCollection<Building> Buildings { get; private set; }

        public List<ArmyUnit> recruitement { get; private set; }
        public Armies army { get; private set; }

        public List<Technology> ResearchedTechnologies { get; private set; }

        public Resources Ressources { get; private set; }
        public string Name { get; private set; }

        private int _tourDepuisCreation;

        public City(string name)
        {
            ResearchedTechnologies = new List<Technology>();
            Name = name;
            Ressources = new Resources(10000, 10000, 10000, 10000, 10000);
            Buildings = new ObservableCollection<Building>();
            Buildings.CollectionChanged += Buildings_CollectionChanged;

            recruitement = new List<ArmyUnit>();
            army = new Army.Armies();
            _tourDepuisCreation = 0;
        }

        //WTF please explain
        void Buildings_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(Buildings.Any(t=>t==null))
                throw new InvalidOperationException();
        }
        public override string ToString()
        {
            return "Ville de " + Name;
        }

        public void RemoveResources(Resources resource)
        {
            Ressources -= resource;
        }

        public void AddResources(Resources resource)
        {
            Ressources += resource;
        }


        public bool TransferResources(City city, Resources resource)
        {
            if (this.Ressources >= resource)
            {
                this.RemoveResources(resource);
                city.AddResources(resource);
                return true;
            }
            return false;
        }

        public bool AddBuilding(int type)
        {
            City city = this;
            var building = BuildingFactory.CreateBuilding(type, ref city);
            return building != null;
        }

        public void RemoveBuilding(int nb)
        {
            int nbrCasern = 0;
            if (Buildings.Count > nb)
            {
                if (Buildings[nb] is Casern)
                {
                    for (int i = 0; i < Buildings.Count; i++)
                    {
                        if (Buildings[nb] is Casern)
                        {
                            nbrCasern++;
                        }
                    }
                    if (nbrCasern == 1)
                    {
                        recruitement.Clear();
                    }
                }
                Buildings.Remove(Buildings[nb]);
            }
        }

        public bool IsBuilt(int bt)
        {
           foreach (Building building in Buildings)
            {
                if (building.ID == (int)bt)
                    return true;
            }
           return false;
        }

        public void Update()
        {
            _tourDepuisCreation++;
            //rsc n'est pas une reelle ressource, c'est une ressource 'theorique'
            //rsc est en fait un multiplicateur, il nous dira de combien multiplier
            //notre constante de base de récupération des ressources

            //Càd que sans rien, une ville gagne 5 de chaque ressource sauf de population
            //Avec une maison, elle gagnera 6 de Meat et 5 du reste, etc<
            List<ArmyUnit> finished = new List<ArmyUnit>();
            foreach (ArmyUnit unit in recruitement)
            {
                unit.Update();
                if (unit.InConstruction == false)
                {
                    army.addUnit(unit);
                    finished.Add(unit);
                }
            }
            foreach (ArmyUnit unit in finished)
            {
                recruitement.Remove(unit);
            }

            Resources rsc = new Resources();
            foreach (Building building in Buildings)
            {
                rsc = rsc + building.Update();
            }
            rsc += BaseProduction();
            Ressources.Update(rsc);
        }

        public bool AddArmy(int type)
        {
            City city = this;
            var armyUnit = ArmyFactory.CreateArmyUnit(type, ref city);
            if (armyUnit != null)
            {
                recruitement.Add(armyUnit);
                return true;
            }
            else
            {
                return false;
            }
        }

        public Building FindBuildingForReschearded(Technology tech)
        {
            return Buildings.ToList().FirstOrDefault(n => !n.AlreadyApplied(tech.ID) && tech.AffectedBuilding.Contains(n.ID));
        }

        public string Attack(Armies BarbarianArmy)
        {
            string Resume = string.Format("La ville est attaqué par des barbares, ils sont {0} \n ", BarbarianArmy.size());

            int armySize = army.size();
            int BarbarianArmySize = BarbarianArmy.size();

            if (army.size() == 0) {
                BarbarianArmy.getUnits().ForEach(n => RemoveResources(n.Transport));
                return "Nous n'avions aucune defense, nous nous somme fait ecraser\n";
            }
            if (BarbarianArmy.Fight(army))
            {
                BarbarianArmy.getUnits().ForEach(n => RemoveResources(n.Transport));
                Resume += string.Format("Nous avons perdu... \n");
            }
            else
            {
                Resume += string.Format("Nous avons gagné! \n");
            }
            Resume += string.Format("Dans la bataille nous avons perdu  {0} soldats et eux {1}", armySize - army.size(), BarbarianArmySize - BarbarianArmy.size());
            return Resume;

        }

        public void TechChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is ObservableCollection<Technology>)
            {
                foreach (var tech in e.NewItems)
                {
                    ResearchedTechnologies.Add((Technology) tech);
                }
            }
        }
    }
}
