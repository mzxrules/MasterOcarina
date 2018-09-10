using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class DropDownStringConverter : StringConverter
{
    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
    {
        return true;
    }

    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
    {
        return true;
    }
}

public class UsageTypeConverter : DropDownStringConverter
{

    public override StandardValuesCollection
                     GetStandardValues(ITypeDescriptorContext context)
    {
        return new StandardValuesCollection(new string[]  
            {
                null,
                "ActorNumber",
                "NextRoomFront",
                "NextRoomBack",
                "PositionX",
                "PositionY",
                "PositionZ",
                "RotationX",
                "RotationY",
                "RotationZ",
                "ChestFlag",
                "SwitchFlag",
                "CollectFlag",
                "Text",
                "Variable"
            });
    }
}

public class ValueTypeConverter : DropDownStringConverter
{
    public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
    {
        return new StandardValuesCollection(new string[]
            {
                "System.UInt16",
                "System.Int16",
                "System.Byte"
            });
    }
}

public class ControlTypeConverter : DropDownStringConverter
{
    public override StandardValuesCollection
                     GetStandardValues(ITypeDescriptorContext context)
    {
        return new StandardValuesCollection(new string[]
        {
            "System.Windows.Forms.TextBox",
            "System.Windows.Forms.ComboBox",
            "System.Windows.Forms.CheckBox"
        });
    }
} 

public partial class ActorDatabaseDefinition
{
    public override string ToString()
    {
        if (Number != null)
            return Number;
        else return IsDefault;
    }
}

public partial class ActorDatabaseDefinitionItem
{
    //controltypes
    public enum Control
    {
        TextBox,
        CheckBox,
        ComboBox,
    }

    public void SetControlType(Control c)
    {
        ControlType = "System.Windows.Forms." + c.ToString();
    }

    public override string ToString()
    {
        return Description;
    }
}

public partial class ActorDatabaseDefinitionItemOption
{
    public override string ToString()
    {
        return string.Format("{0} {1}", Value, Description);
    }
}