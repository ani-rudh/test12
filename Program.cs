﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using Xbim.Ifc;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.Extensions;
using Xbim.Ifc4.Interfaces;
using Xbim.Common.Geometry;
using Xbim.ModelGeometry.Scene;
using Xbim.Presentation;
using Xbim.XbimExtensions;
using Xbim.Common.XbimExtensions;
using System.Xml;
using System.Xml.Linq;
namespace Test
{
    class Program
    {
        //const string file = "D:\\THESIS_ANI\\xBIM\\2011-09-14-Duplex-IFC\\Duplex_A_20110907_optimized.ifc";
        //const string file = "D:\\THESIS_ANI\\xBIM\\trial_openings_IFC_2x3coord.ifc";
        //const string file = "D:\\THESIS_ANI\\xBIM\\simplegeometry_trial.ifc";
        /***********************************************************************************************************************************/
        //const string file = "F:\\TUM\\STUDIES\\THESIS\\BIMtoUnity\\xBIM\\IFC_samplefiles\\2011-09-14-Clinic-IFC\\trialbim_simple.ifc";
        //const string file = "F:\\TUM\\STUDIES\\THESIS\\BIMtoUnity\\xBIM\\IFC_samplefiles\\2011-09-14-Clinic-IFC\\simplegeometry_openings_2x3.ifc";
        //const string file = "‪C:\\Users\\aniru\\Desktop\\exam.ifc";
        //const string file = "C:\\Users\\aniru\\Desktop\\XBIM\\simplegeometry_trial.ifc";
        const string file = "C:\\Users\\aniru\\Desktop\\XBIM\\SimpleGeom_Openings_2x3_coord.ifc";
        //const string file = "C:\\Users\\aniru\\Desktop\\XBIM\\.SimpleGeom_Openings_4_DTV.ifc";
        //const string file = "F:\\TUM\\STUDIES\\THESIS\\BIMtoUnity\\xBIM\\IFC_samplefiles\\2011-09-14-Clinic-IFC\\simplegeometry_openings_4.ifc";
        //const string file = "F:\\TUM\\STUDIES\\THESIS\\BIMtoUnity\\xBIM\\IFC_samplefiles\\2011-09-14-Clinic-IFC\\20160414office_model_CV2_fordesign.ifc";
        //const string file = "F:\\TUM\\STUDIES\\THESIS\\BIMtoUnity\\xBIM\\IFC_samplefiles\\2011-09-14-Clinic-IFC\\wall.ifc";

        //static Dictionary<int, Dictionary<int,Dictionary<int,Dictionary<int,List<int>>>>> temp;

        
            
        //create the xmlWriter as a static variable in order to use it everywhere in the code.
        static XmlTextWriter xmlWriter;
       
        //Use automatic indentation for readability.
       
 

        public static void Main()
        {



            using (var model = IfcStore.Open(file))
        {

            
            Console.WriteLine("\n" + "---------------------------------------S T A R T---------------------------------------" + "\n");
            Dictionary<string, IfcSpace> spaceids;
            Dictionary<string, IfcBuildingStorey> storeyids;

            var project = model.Instances.FirstOrDefault<IIfcProject>();

            IEnumerable<IfcSpace> spaces = model.Instances.OfType<IfcSpace>();
            spaceids = getspaceelementids(spaces);

            IEnumerable<IfcBuildingStorey> storeys = model.Instances.OfType<IfcBuildingStorey>();
            storeyids = getstoreyelementids(storeys);

            var context = new Xbim3DModelContext(model);
            context.CreateContext();

            var productshape = context.ShapeInstances();

            var _productShape = context.ShapeInstances().Where(s => s.RepresentationType != XbimGeometryRepresentationType.OpeningsAndAdditionsExcluded).ToList();

                //name of the model
                var name_of_model = file.Split(new char[] { '\\' }).Last();

                //number of shapes in the model
                var number_of_shapes = _productShape.Count();

                Console.WriteLine("OPENED MODEL : " + name_of_model + " | No of shape Instances in the model is : " + number_of_shapes + "\n");


                //get the name of the model without the ifc extention
                var name_of_file = name_of_model.Split('.')[0];

                //creating the xml file in the project directory named after the name of the model
                xmlWriter = new XmlTextWriter(name_of_file + ".xml", null);

                //in order to have the correct xml format 
                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.WriteStartDocument();
                //writing the start element Shapes with his attribut NumShapes
                xmlWriter.WriteStartElement("Shapes");
                xmlWriter.WriteAttributeString("NumShapes",number_of_shapes.ToString());


                PrintHierarchy(project, 0, spaceids, storeyids, _productShape, number_of_shapes, context);
                
                
                xmlWriter.WriteEndDocument();
                xmlWriter.Close();
            Console.WriteLine("\n" + "---------------------------------------E N D---------------------------------------" + "\n");
                /********************************************************************************/
                
            }
            Console.ReadLine();
        }
        
        private static void PrintHierarchy(IIfcObjectDefinition o, int level, Dictionary<string, IfcSpace> spaceidset, Dictionary<string, IfcBuildingStorey> storeyidset, List<XbimShapeInstance> _shapes, int number_of_shapes, Xbim3DModelContext mod_context)
    {
        Console.WriteLine($"{GetIndent(level)}{" >> " + o.Name} [{o.GetType().Name}{ " | #" + o.EntityLabel  }] {"\n"}");
        var item = o.IsDecomposedBy.SelectMany(r => r.RelatedObjects);

        foreach (var i in item)
        {

            var id = i.GlobalId.ToString();

            //Console.WriteLine("------------------Matches found :" + _si.ShapeGeometryLabel.ToString());

            PrintHierarchy(i, level + 2, spaceidset, storeyidset, _shapes, number_of_shapes, mod_context);

            //Console.WriteLine("Instance ID: " + eid);

            if (spaceidset.ContainsKey(id))
            {
                IfcSpace spacenode;
                spaceidset.TryGetValue(id, out spacenode);
                var spacenodelelems = spacenode.GetContainedElements();

                if (spacenodelelems.Count() > 0)
                {
                    Console.WriteLine($"{GetIndent(level + 4)}" + "OBJECTS FOUND UNDER SPACE ARE: \n");
                    foreach (var sne in spacenodelelems)
                    {
                        var parent = sne.IsContainedIn;
                        var eid = sne.EntityLabel.ToString();

                        Console.WriteLine($"{GetIndent(level + 5)}{" --> " + sne.Name} [{sne.GetType().Name}{ " | #" + sne.EntityLabel }{" | PARENT : #" + parent.EntityLabel}]");
                        Console.WriteLine(sne.EntityLabel);
                            var si = _shapes.Find(x => x.IfcProductLabel.ToString() == eid);
                        //Console.WriteLine("------------------Matches found :" + si.ShapeGeometryLabel.ToString());
                        getgeometry(si, mod_context);
                    }
                }
            }

            else if (storeyidset.ContainsKey(id))
            {
                IfcBuildingStorey bsnode;
                storeyidset.TryGetValue(id, out bsnode);
                var bsnodelelems = bsnode.GetContainedElements();

                if (bsnodelelems.Count() > 0)
                {
                    Console.WriteLine($"{GetIndent(level + 4)}" + "OTHER OBJECTS FOUND UNDER STOREY ARE: \n");



                    foreach (var bsne in bsnodelelems)
                    {
                        var parent = bsne.IsContainedIn;
                        var eid = bsne.EntityLabel.ToString();

                        var name_of_shape = bsne.GetType().Name;

                        Console.WriteLine($"{GetIndent(level + 5)}{" --> " + bsne.Name} [{name_of_shape}{ " | #" + bsne.EntityLabel } {" | PARENT : #" + parent.EntityLabel }]");
                        Console.WriteLine("]]]]]]]]]]"+bsne.EntityLabel);


                            
                          

                            var si = _shapes.Find(x => x.IfcProductLabel.ToString() == eid);

                            //write the name of the shape  with its id
                            xmlWriter.WriteStartElement(name_of_shape);
                            xmlWriter.WriteAttributeString("ID", bsne.EntityLabel.ToString());
                            
                        
                            getgeometry(si, mod_context,bsne.EntityLabel, number_of_shapes);

                            // for each XML element that we created we should close it in order to have the correct hierarchy in the xml file
                            xmlWriter.WriteEndElement();

                        }
                }

            }


            /***************************************************************************/

        }

    }

        private static void getgeometry(XbimShapeInstance si, Xbim3DModelContext mod_context)
        {
            throw new NotImplementedException();
        }




        private static string GetIndent(int level)
    {
        var indent = "";
        for (int i = 0; i < level; i++)
            indent += "  ";
        return indent;
    }

    private static Dictionary<string, IfcSpace> getspaceelementids(IEnumerable<IfcSpace> spaces_ien)
    {
        Dictionary<string, IfcSpace> eids = new Dictionary<string, IfcSpace>();
        foreach (IfcSpace s in spaces_ien)
        {
            eids.Add(s.GlobalId.ToString(), s);
            //Console.WriteLine("Gid for " + s.Name + " is: " +s.GlobalId.ToString());
        }

        return eids;
    }

    private static Dictionary<string, IfcBuildingStorey> getstoreyelementids(IEnumerable<IfcBuildingStorey> storeys_ien)
    {
        Dictionary<string, IfcBuildingStorey> eids = new Dictionary<string, IfcBuildingStorey>();
        foreach (IfcBuildingStorey s in storeys_ien)
        {
            eids.Add(s.GlobalId.ToString(), s);
            //Console.WriteLine("Gid for " + s.Name + " is: " +s.GlobalId.ToString());
        }

        return eids;
    }

    private static void getgeometry(XbimShapeInstance shape, Xbim3DModelContext m_context, int entityLabel, int number_of_shapes)
    {

        XbimShapeTriangulation mesh = null;
        
        var geometry = m_context.ShapeGeometry(shape);
            

        Console.WriteLine($"{"\n"}{GetIndent(11)}{"--Geometry Type: " + geometry.Format}");


        var ms = new MemoryStream(((IXbimShapeGeometryData)geometry).ShapeData);
        var br = new BinaryReader(ms);

        mesh = br.ReadShapeTriangulation();
        mesh = mesh.Transform(((XbimShapeInstance)shape).Transformation);

        var facesfound = mesh.Faces.ToList();


            var number_of_faces = facesfound.Count();

        Console.WriteLine($"{"\n"}{GetIndent(11)}{"  -----No. of faces on the shape #" + shape.IfcProductLabel + ": " + number_of_faces}");

            //used for an ID for each face
            int face_index = 0;
            //used for the total number of triangles
            int number_of_triangles = 0;
            
            //write the Faces element with its count
            xmlWriter.WriteStartElement("Faces");
            xmlWriter.WriteAttributeString("NumFaces", number_of_faces.ToString());
          
            foreach (XbimFaceTriangulation f in facesfound)
        {

                number_of_triangles = f.TriangleCount;
            Console.WriteLine($"{"\n"}{GetIndent(13)}{"  -----Triangle count on face: " + f.GetType() + " :mesh is  " + number_of_triangles}");
                
                
                
                face_index++;

                composetrianglesets(f, mesh, entityLabel, facesfound.Count(), face_index, number_of_triangles, number_of_shapes);


        }
            
            //this ends the faces element in the xml file
            xmlWriter.WriteEndElement();
           
            
            
            
            //Console.WriteLine($"{"\n"}{GetIndent(13)}{" -----Vertices of the shape: #" + shape.IfcProductLabel}");
            //foreach (var v in mesh.Vertices.ToList())
            //{
            //    Console.WriteLine($"{GetIndent(13)}{" --vertex_" + mesh.Vertices.ToList().IndexOf(v) + " : " + Math.Round((double)v.X, 2) + " | " + Math.Round((double)v.Y, 2) + " | " + Math.Round((double)v.Z, 2)}");

            //}

            Console.WriteLine("\n");
    }

       

        private static void composetrianglesets(XbimFaceTriangulation face, XbimShapeTriangulation shapemesh, int entityLabel, int Number_Faces, int face_index, int triangle_Count, int number_of_shapes)
    {


           
            Dictionary<string, List<int>> triangles = new Dictionary<string, List<int>>();
        Dictionary<string, XbimPoint3D> vertices = new Dictionary<string, XbimPoint3D>();

        List<XbimPoint3D> verts = shapemesh.Vertices.ToList();

            //write each face with its ID
            xmlWriter.WriteStartElement("Face");
            xmlWriter.WriteAttributeString("ID",face_index.ToString());


            //write the Triangles with its number for each face
            xmlWriter.WriteStartElement("Triangles");
            xmlWriter.WriteAttributeString("NumTriangles", triangle_Count.ToString());




            //for (int i = 0; i < verts.Count(); i++)
            //{
            //    string name = "vertex_" + (i).ToString();
            //    vertices.Add(name, verts[i]);

            //}
            //foreach (var v in vertices)
            //{
            //    Console.WriteLine($"{GetIndent(15)}{v.Key + ": "}{v.Value.X + ", "}{v.Value.Y + ", "}{v.Value.Z}");
            //}
            /*******************************************************************************************************/


            for (int i = 0; i < face.TriangleCount; i++)
        {
            string name = "triangle_" + (i + 1).ToString();
               
                triangles.Add(name, face.Indices.ToList().GetRange(i * 3, 3));
         }
           
            //for the id of the triangle
            int triangle_index = 0;
        foreach (var x in triangles)
            {
                //writing each triangle with his ID
                xmlWriter.WriteStartElement("Triangle");
                xmlWriter.WriteAttributeString("ID", triangle_index.ToString());


                var vert1 = x.Value[0];
            var vert2 = x.Value[1];
            var vert3 = x.Value[2];
            Console.WriteLine($"{"\n"}{GetIndent(15)}{x.Key + ": "}{vert1 + ","}{vert2 + ","}{vert3}");
            Console.WriteLine($"{GetIndent(15)}{"---------------------"}");

                //writing the vertices element with its count
                xmlWriter.WriteStartElement("Vertices");
                xmlWriter.WriteAttributeString("NumVertices", "3");
                Double X ;
                Double Y ;
                Double Z ;
                for (int y = 0; y < x.Value.Count(); y++)
                {

                    //get the vertice index(ID) and its x,y,z
                    var vertice_index = x.Value[y];

                    X = Math.Round((double)verts[x.Value[y]].X,2);
                    Y = Math.Round((double)verts[x.Value[y]].Y, 2);
                    Z = Math.Round((double)verts[x.Value[y]].Z, 2);


                    Console.WriteLine($"{GetIndent(15)}{vertice_index.ToString() + ": "}{X}{"|"}{Y}{"|"}{Z}");

                    //writing in the xml file
                    xmlWriter.WriteStartElement("Vertice");

                    xmlWriter.WriteAttributeString("ID", vertice_index.ToString());

                    xmlWriter.WriteElementString("X", X.ToString());
                    xmlWriter.WriteElementString("Y", Y.ToString());
                    xmlWriter.WriteElementString("Z", Z.ToString());
                    
                    //this is for the vertice_index
                    xmlWriter.WriteEndElement();
                }
                //this is for the  vertices
                xmlWriter.WriteEndElement();
             
                //this is for the triangle id
                   xmlWriter.WriteEndElement();

                triangle_index++;
             
            }

            //this is for the triangle Number
            xmlWriter.WriteEndElement();

          
            //this is for the face id
            xmlWriter.WriteEndElement();
     }


        public static void Append(string filename, string firstName)
        {
            var contact = new XElement("contact", new XElement("firstName", firstName));
            var doc = new XDocument();

            if (File.Exists(filename))
            {
                doc = XDocument.Load(filename);
                doc.Element("contacts").Add(contact);
            }
            else
            {
                doc = new XDocument(new XElement("contacts", contact));
            }
            doc.Save(filename);
        }
    }
}
