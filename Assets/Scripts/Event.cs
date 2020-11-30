
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(fileName = "New Event", menuName = "Event")]
public class Event : ScriptableObject
{
    public enum EventType{
        Normal,
        Chain,
        Triggered
    }
    public enum TriggerReasons
    {
        LowFood,
        LowMoney,
        ArmyRevolution,
        LowArmyPower
    }
    public string eventName;
    [TextArea(15, 20)]
    public string eventDescription;
    public string selection1Description;
    public int[] effect1 = new int[8];
    public string selection2Description;
    public int[] effect2 = new int[8];
    public EventType type;
    public int chainID;
    public int chainOrder;
    public int[] eventGap;
    public TriggerReasons triggerReason;
}
