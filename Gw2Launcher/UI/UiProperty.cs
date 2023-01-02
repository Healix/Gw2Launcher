using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Gw2Launcher.UI
{
    public class UiPropertyColor : UiPropertyValue
    {
        public UiPropertyColor(string property = null)
            : base(property, UiColors.Colors.Custom)
        {

        }
    }

    public class UiPropertyValue : Attribute
    {
        public const int TYPEID = 1448110421;

        public UiPropertyValue(string property, object value)
        {
            this.PropertyName = property;
            this.Value = value;
        }

        public string PropertyName
        {
            get;
            private set;
        }

        public object Value
        {
            get;
            private set;
        }

        public override object TypeId
        {
            get
            {
                return TYPEID;
            }
        }
    }

    public class UiTypeDescriptionProvider : TypeDescriptionProvider
    {
        public UiTypeDescriptionProvider()
            : base(TypeDescriptor.GetProvider(typeof(object))) 
        { 
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return new UiTypeDescriptor(base.GetTypeDescriptor(objectType, instance));
        }
    }

    public class UiTypeDescriptor : ICustomTypeDescriptor
    {
        ICustomTypeDescriptor td;

        public UiTypeDescriptor(ICustomTypeDescriptor td)
        {
            this.td = td;
        }

        public AttributeCollection GetAttributes()
        {
            return td.GetAttributes();
        }

        public string GetClassName()
        {
            return td.GetClassName();
        }

        public string GetComponentName()
        {
            return td.GetComponentName();
        }

        public TypeConverter GetConverter()
        {
            return td.GetConverter();
        }

        public EventDescriptor GetDefaultEvent()
        {
            return td.GetDefaultEvent();
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return td.GetDefaultProperty();
        }

        public object GetEditor(Type editorBaseType)
        {
            return td.GetEditor(editorBaseType);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return td.GetEvents(attributes);
        }

        public EventDescriptorCollection GetEvents()
        {
            return td.GetEvents();
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var collection = td.GetProperties(attributes);
            var properties = new PropertyDescriptor[collection.Count];

            for (var i = 0; i < properties.Length; i++)
            {
                if (properties[i] != null)
                    continue;

                var p = collection[i];
                var a = GetUiPropertyValue(p.Attributes);

                if (a != null)
                {
                    var n = p.Name;

                    if (n.EndsWith("Name"))
                        n = n.Substring(0, n.Length - 4);
                    else
                        n = null;

                    properties[i] = new UiPropertyDescriptor(this, p, n == null ? n : n + "+");

                    if (a.PropertyName != null)
                    {
                        n = a.PropertyName;
                    }

                    if (n != null)
                    {
                        var p2 = collection[n];

                        if (p2 != null)
                        {
                            var j = collection.IndexOf(p2);

                            if (j != -1 && j != i)
                            {
                                properties[j] = new UiLinkedPropertyDescriptor(this, p2, properties[i], a);
                            }
                        }
                    }
                }
                else
                {
                    properties[i] = p;
                }
            }

            return new PropertyDescriptorCollection(properties);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return td.GetPropertyOwner(pd);
        }

        private UiPropertyValue GetUiPropertyValue(Attribute a)
        {
            if (a is UiPropertyValue)
            {
                return (UiPropertyValue)a;
            }
            else
            {
                try
                {
                    var t = a.GetType();
                    var n = (string)t.GetProperty("PropertyName").GetValue(a);
                    var v = t.GetProperty("Value").GetValue(a);

                    return new UiPropertyValue(n, v);
                }
                catch
                {
                    return null;
                }
            }
        }

        private UiPropertyValue GetUiPropertyValue(AttributeCollection attributes)
        {
#if DEBUG
            //note the designer will cache the provider on load and further builds will be a different assembly, so the classes can't be directly compared
            //this isn't needed to run and only prevents incorrect displays while designing

            foreach (Attribute a in attributes)
            {
                if (object.Equals(a.TypeId, UiPropertyValue.TYPEID))
                {
                    var guid = a.GetType().GUID;

                    if (guid.Equals(typeof(UiPropertyValue).GUID) || guid.Equals(typeof(UiPropertyColor).GUID))
                    {
                        return GetUiPropertyValue(a);
                    }
                }
            }

            return null;
#else
            return (UiPropertyValue)attributes[typeof(UiPropertyValue)];
#endif
        }
    }

    public class UiPropertyDescriptor : PropertyDescriptor
    {
        protected PropertyDescriptor pd;
        protected UiTypeDescriptor td;
        protected string displayName;

        public UiPropertyDescriptor(UiTypeDescriptor td, PropertyDescriptor pd, string displayName = null)
            : base(pd)
        {
            this.td = td;
            this.pd = pd;
            this.displayName = displayName;
        }

        public override Type ComponentType
        {
            get
            {
                return pd.ComponentType;
            }
        }

        public override bool CanResetValue(object component)
        {
            return pd.CanResetValue(component);
        }

        public override object GetValue(object component)
        {
            return pd.GetValue(component);
        }

        public override bool IsReadOnly
        {
            get
            {
                return pd.IsReadOnly;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return pd.PropertyType;
            }
        }

        public override string DisplayName
        {
            get
            {
                if (displayName != null)
                    return displayName;
                return base.DisplayName;
            }
        }

        public override void ResetValue(object component)
        {
            pd.ResetValue(component);
        }

        public override void SetValue(object component, object value)
        {
            pd.SetValue(component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return pd.ShouldSerializeValue(component);
        }
    }

    public class UiLinkedPropertyDescriptor : UiPropertyDescriptor
    {
        protected PropertyDescriptor linked;
        protected UiPropertyValue value;

        public UiLinkedPropertyDescriptor(UiTypeDescriptor td, PropertyDescriptor pd, PropertyDescriptor linked, UiPropertyValue value)
            : base(td, pd)
        {
            this.linked = linked;
            this.value = value;
        }

        public override bool ShouldSerializeValue(object component)
        {
            var b = pd.ShouldSerializeValue(component);

            if (b)
            {
                if (!object.Equals(linked.GetValue(component), value.Value))
                {
                    return false;
                }
            }

            return b;
        }
    }

    class UiColorTypeEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override bool IsDropDownResizable
        {
            get
            {
                return true;
            }
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var s = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));

            if (s != null)
            {
                var panel = new Panel()
                {
                    Size = new Size(200, 150),
                };

                var grid = new Controls.ScaledDataGridView()
                {
                    BorderStyle = BorderStyle.None,
                    CellBorderStyle = DataGridViewCellBorderStyle.None,
                    RowHeadersVisible = false,
                    ColumnHeadersVisible = false,
                    AllowUserToResizeRows = false,
                    AllowUserToResizeColumns = false,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    AllowUserToOrderColumns = false,
                    EditMode = DataGridViewEditMode.EditProgrammatically,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    BackgroundColor = SystemColors.Window,
                    MultiSelect = false,
                    ScrollBars = ScrollBars.Vertical,
                    Dock = DockStyle.Fill,
                };

                grid.SortCompare += grid_SortCompare;
                grid.CellPainting += grid_CellPainting;

                grid.SuspendLayout();

                grid.Columns.Add("", "");
                grid.Columns.Add("", "");

                grid.Columns[0].Width = 20;
                grid.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

                foreach (UiColors.Colors c in Enum.GetValues(typeof(UiColors.Colors)))
                {
                    var r = new DataGridViewRow();
                    r.CreateCells(grid);

                    r.Height = 18;

                    r.Cells[0].Value = UiColors.GetColor(c);
                    r.Cells[1].Value = c;

                    grid.Rows.Add(r);

                    if (object.Equals(c, value))
                    {
                        r.Cells[1].Selected = true;
                    }
                }

                grid.Sort(grid.Columns[1], ListSortDirection.Ascending);

                panel.Controls.Add(grid);

                grid.ResumeLayout();

                grid.CellMouseUp += delegate
                {
                    foreach (DataGridViewRow r in grid.SelectedRows)
                    {
                        value = r.Cells[1].Value;
                    }

                    s.CloseDropDown();
                };

                s.DropDownControl(panel);
            }

            return value;
        }

        void grid_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.Column.Index == 1)
            {
                var c1 = (UiColors.Colors)e.CellValue1;
                var c2 = (UiColors.Colors)e.CellValue2;

                e.Handled = true;

                if (c1 != c2)
                {
                    if (c1 == UiColors.Colors.Custom)
                    {
                        e.SortResult = -1;
                        return;
                    }
                    else if (c2 == UiColors.Colors.Custom)
                    {
                        e.SortResult = 1;
                        return;
                    }
                }

                e.SortResult = c1.ToString().CompareTo(c2.ToString());
            }
        }

        void grid_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                if (e.Value is Color)
                {
                    var g = e.Graphics;
                    var c = (Color)e.Value;
                    var b = Rectangle.Inflate(e.CellBounds, -1, -3);

                    e.PaintBackground(b, true);

                    using (var brush = new SolidBrush(c))
                    {
                        g.FillRectangle(brush, b);
                        g.DrawRectangle(Pens.Black, b);
                    }

                    e.Handled = true;
                }
            }
        }
    }

    class UiColorTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return value.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                try
                {
                    return Enum.Parse(typeof(UiColors.Colors), (string)value, true);
                }
                catch
                {
                    return UiColors.Colors.Custom;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }
    }

}
