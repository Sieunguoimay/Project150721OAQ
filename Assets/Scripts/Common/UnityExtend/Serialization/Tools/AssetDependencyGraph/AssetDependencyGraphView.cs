using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Text3D.Scripts;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class AssetDependencyGraphView : GraphView
{
    public AssetDependencyGraphView()
    {
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());
        this.AddManipulator(new ContentZoomer());

        AddElement(CreateNode());
        AddElement(CreateNode());
        AddElement(CreateNode());
    }

    private Node CreateNode()
    {
        var node = new Node()
        {
            title = "Hello world of GraphView",
        };
        //node.style.flexDirection = FlexDirection.Column;
        //node.ElementAt(0).BringToFront();
        for (var i = 0; i < node.childCount; i++)
        {
            //node.ElementAt(i).BringToFront();
            //UnityEngine.Debug.Log();
        }


        CreateOutputPort(node);
        CreateInputPort(node);
        node.RefreshExpandedState();
        node.RefreshPorts();

        CustomizeNode(node);

        return node;
    }
    private static void CustomizeNode(Node node)
    {
        var input = node.Q<VisualElement>("input");
        var output = node.Q<VisualElement>("output");
        var title = node.Q<VisualElement>("title");
        var top = node.Q<VisualElement>("top");
        var nodeBorder = node.Q<VisualElement>("node-border");
        var contents = node.Q<VisualElement>("contents");

        var label = new TextElement() { text = "Hello unity UIElements", name = "zo" };
        label.style.marginLeft = 5;
        label.style.marginRight = 5;
        label.style.alignContent = Align.Center;

        title.parent.Remove(title);
        input.parent.Remove(input);
        output.parent.Remove(output);
        top.parent.Remove(top);
        contents.parent.Remove(contents);

        nodeBorder.Add(input);
        nodeBorder.Add(label);
        nodeBorder.Add(output);


        //CustomizeInputPort(input);
        //CustomizeOuputPort(output);

        //input.parent.hierarchy.Remove(input);
        //contents.hierarchy.Add(input);
        //input.PlaceBehind(top);
    }
    private static void CustomizeInputPort(VisualElement port)
    {

        //port.style.alignItems = Align.Center;
        //port.style.paddingTop = 0;
        //port.style.paddingBottom = 0;

        var p = port.Q<Port>();
        //p.style.paddingLeft = 0;
        //p.style.paddingRight = 0;
        ////p.style.flexDirection = FlexDirection.Column;
        //p.style.alignSelf = Align.Center;

        ////p.style.alignItems = Align.FlexStart;
        //var connector = port.Q<VisualElement>("connector");
        //connector.parent.Remove(connector);
        //connector.style.alignSelf = Align.FlexStart;
        ////connector.style.position = Position.Absolute;
        ////connector.style.top = 0;
        ////connector.style.left = 0;
        //connector.style.marginLeft = 0;
        //connector.style.marginRight = 0;

        //p.style.width = connector.style.width;
        //p.style.height = connector.style.height;

        //var type = port.Q<VisualElement>("type");
        //type.style.position = Position.Absolute;

        //type.style.alignSelf = Align.FlexEnd;
        //type.style.display = DisplayStyle.None;
    }
    private static void CustomizeOuputPort(VisualElement port)
    {
        //port.style.alignItems = Align.Center;
        //port.style.paddingTop = 0;
        //port.style.paddingBottom = 0;

        var p = port.Q<Port>();
        p.style.flexDirection = FlexDirection.Row;
        //p.style.paddingLeft = 0;
        //p.style.paddingRight = 0;
        //p.style.alignItems = Align.FlexEnd;
        //p.style.flexDirection = reverse ? FlexDirection.ColumnReverse : FlexDirection.Column;

        //var connector = port.Q<VisualElement>("connector");
        //connector.style.alignSelf = Align.FlexEnd;

        //var type = port.Q<VisualElement>("type");
        //type.style.display = DisplayStyle.None;
    }

    private Port CreateOutputPort(Node node)
    {
        var port = node.InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
        port.portName = "";
        node.outputContainer.Add(port);
        return port;
    }
    private Port CreateInputPort(Node node)
    {
        var port = node.InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
        port.portName = "";
        node.inputContainer.Add(port);
        return port;
    }
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();
        ports.ForEach(port =>
        {
            if (startPort != port
            && startPort.node != port.node
            && startPort.direction != port.direction
            && !port.connections.Any(e=>e.input == startPort||e.output ==startPort)
            )
            {
                compatiblePorts.Add(port);
            }
        });
        return compatiblePorts;
    }
}