using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;

    public class XmlObjectLoader : MonoBehaviour {
    Dictionary<string, string> obj;

    string filename= "C:\\Users\\aniru\\Documents\\testUnity\\Assets\\SimpleGeom_Openings_2x3_coord.xml";

    //we couldve did another solution: to create the dict in the source file and store it as serializable object and then using in this project but i did this way so we 
    //you can check the sturture of the model in a clearer way


  static Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string,string>>>>> Shapes;

    // Use this for initialization
    void Start () {
        Debug.Log("---------------------------STARTTTT-----------------------------------------------");
        //to extract the file from the xml file
        extract();
        Debug.Log("----------The xml file has been extracted and the info is stored in a dict--------");
        //to be able to read the info from the dict created where all the info are.
        build(Shapes);       
    }

    
    /*
     * This function  we access the dictionnary containing all the xml file info and we can biuil the mesh based
     * on these info
     * the strcuture of the dict is as follows:
     * 
     * Shapes||Faces||triangles||vertices||coordinates
     * 
     * 
     * */
    //i
    public void build(Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>> Shapes)
    {
        HashSet<string> val = new HashSet<string>();

        foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>> shape_type in Shapes)
        {
            //we are looping throufght the different shapes of the model

            Debug.Log("--> The shape is a " + shape_type.Key);
            Dictionary <string,Dictionary<string,Dictionary <string, Dictionary<string, string>>>> faces= shape_type.Value;

            foreach (KeyValuePair<string,Dictionary<string, Dictionary<string, Dictionary<string, string>>>> face in faces)
            {
                //we are looping throufght the faces of each shape

                Debug.Log("--> The ID of the face is " + face.Key);
                Dictionary<string, Dictionary<string, Dictionary<string, string>>> triangles = face.Value;
                val = new HashSet<string>();


               
                //int j=0;
                int j = 0;
                foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, string>>> triangle in triangles)
                {
                    
                    //we are looping throufght the triangles of each face

                    Debug.Log("--> The ID of the triangle is "+ triangle.Key);
                    Dictionary<string, Dictionary<string, string>> vertices = triangle.Value;

                    Vector3[] Vertices = new Vector3[4];
                    int i = 0;



                   



                    
                    foreach (KeyValuePair<string, Dictionary<string, string>> coord in vertices)
                    {

                        //we are looping throught the vertices of each face

                       Dictionary<string, string> coor = coord.Value;
                       
                       Vertices[i] = new Vector3(float.Parse(coor["X"]), float.Parse(coor["Y"]),float.Parse(coor["Z"]));
                       i++;
                        var tp = coor["X"] + "_" + coor["Y"] + "_" + coor["Z"];
                        Debug.Log("----Vertice " + i);
                        Debug.Log("X :" + coor["X"]);
                        Debug.Log("Y :" + coor["Y"]);
                        Debug.Log("Z :" + coor["Z"]);
                        //adding the values to a hashset to not repeat the same values.
                        val.Add(tp);
                        j++;
                        
                        Debug.Log(tp+"---------------");
                    }

                    
                }


                Debug.Log(val.Count);
                Debug.Log("========The vertices of the face" + face.Key);
                int l = 0;

                Vector3[] Vertices_face = new Vector3[val.Count];
                foreach (var a in val)
                {

                    Debug.Log(a);
                    var c = a.Split('_');
                    
                    float X = float.Parse(c[0]);
                    float Y = float.Parse(c[1]);
                    float Z = float.Parse(c[2]);
                    //we are getting the four vertices of the face
                 
                    Vertices_face[l] = new Vector3(X, Y, Z);

                    l=l+1;
                   
                }
                Debug.Log("The number of the vertices in this face is " + val.Count);
            }


        }




        }


    /*
    * This function  reads the xml file and store the results in a dictionnary.
    * 
    * the strcuture of the dict is as follows:
    * 
    * Shapes||Faces||triangles||vertices||coordinates
    * 
    * 
    * */
    public void extract()
    {

        Shapes = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>>();

        XmlDocument xmlDoc = new XmlDocument(); // xmlDoc is the new xml document.
        xmlDoc.Load(filename); // load the file.
        XmlNodeList shapes_node = xmlDoc.GetElementsByTagName("Shapes"); // array of the level nodes.
        XmlNode shape_node = shapes_node.Item(0);

        Debug.Log("Name of the head element :"+shape_node.Name);

        //shape_list contains all the shapes in the xml file
        XmlNodeList shapes_list= shape_node.ChildNodes;

        
        foreach (XmlNode shape_children in shapes_list)
        {
            //we re going through each shape to get all the faces 
            var shape_name = shape_children.Name+"_"+shape_children.Attributes["ID"].Value;
            Debug.Log("Name of the shape is :"+ shape_name);
            Shapes.Add(shape_name,new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, string>>>>());
            XmlNode faces = shape_children.FirstChild;
            XmlNodeList faces_lists = faces.ChildNodes;

           

            foreach (XmlNode face_children in faces_lists)
            {
                //going through each face

                var face_index = face_children.Attributes["ID"].Value;
                Debug.Log("ID of the face is :" + face_index);
           
                //adding the face to the dictionnary
                Shapes[shape_name].Add(face_index,new Dictionary<string, Dictionary<string, Dictionary<string, string>>>());

                //getting the childnode of the face which is the triangles
                XmlNode triangles = face_children.FirstChild;
                XmlNodeList triangles_lists = triangles.ChildNodes;

                //in order to access the embeded dictionnaries
                var temp = Shapes[shape_name];
                var temp1 = temp[face_index];

                foreach (XmlNode triangle_children in triangles_lists)
                {

                    //to get each triangle in the face
                    var triangle_index = triangle_children.Attributes["ID"].Value;
                    Debug.Log("ID of the triangle is :" + triangle_index);
                    Debug.Log("------------");

                    //adding the triangle index to the dictionnary   
                    temp1.Add(triangle_index,new Dictionary<string, Dictionary<string, string>>());
                    
                    //getting the child nodes of each triangle which are the vertices 
                    XmlNode vertices = triangle_children.FirstChild;
                    XmlNodeList vertices_lists = vertices.ChildNodes;

                    //accessing the embeded dictionnary
                    var temp2=temp1[triangle_index];

                    foreach (XmlNode vertice_children in vertices_lists)
                    {
                        //to get each vertice in the triangle
                        var vertice_index = vertice_children.Attributes["ID"].Value;

                        //adding the triangle to the dictionnary
                        temp2.Add(vertice_index,new Dictionary<string, string>());
                      
                        Debug.Log("ID of the vertice is :" + vertice_index);

                        XmlNodeList coordinates_list = vertice_children.ChildNodes;

                        var temp3 = temp2[vertice_index];
                        foreach (XmlNode coordinate in coordinates_list)
                        {
                            //getting the coordinates of each vertice
                            temp3.Add(coordinate.Name, coordinate.InnerText.ToString());
                            Debug.Log("Value of the "+ coordinate.Name + " is : "+coordinate.InnerText);


                        }
                      
                    }
                }

        }
    }

}



    // Update is called once per frame
    void Update () {
		
	}
}
