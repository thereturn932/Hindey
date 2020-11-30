using System.Collections.Generic;

[System.Serializable]
public class Country
{
    public string countryName;
    public int money;
    public int food;
    public float sides;
    public float armyPower;
    public int population;
    public int happiness;
    public int relationWithCountryA;
    public int relationWithCountryB;
    public int day;
    public int tax;
    public int[] decisionModifiers = new int[8];
    public List<Building> myBuildings = new List<Building>();
    public Event[] events;

    public Country()
    {

    }

    public Country(string countryName, int gameLength)
    {
        this.countryName = countryName;
        this.money = 1000;
        this.food = 100;
        this.sides = 50.0f;
        this.armyPower = 60.0f;
        this.population = 1000;
        this.happiness = 70;
        this.relationWithCountryA = 50;
        this.relationWithCountryB = 50;
        this.tax = 1;
        this.day = 1;
        this.events = new Event[gameLength];
    }
}
