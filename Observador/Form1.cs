using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Observador.Form1;
using Microsoft.VisualBasic;

namespace Observador
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ibm = new IBM("IBM");
        }
        IBM ibm;
        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.MultiSelect = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView2.MultiSelect = false;
            dataGridView2.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ibm.Subscribir(new Inversionista(Interaction.InputBox("DNI: "),
                                            Interaction.InputBox("Nombre: "),
                                            Interaction.InputBox("Apellido: ")));
            Mostrar(dataGridView1, ibm.RetornarInversionistas());
        }
        private void Mostrar(DataGridView pDGV, Object pO)
        {
            pDGV.DataSource = null; //anulo cualquier referencia previa que pueda tener
            pDGV.DataSource = pO;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //Boton para cotizacion. Cargar una cotizacion a traves de la propiedad cotizacion

            //SI le cargo una cotizacion quiero llamar al metodo dataGridView1_RowEnter
            try
            {
                string cot = Interaction.InputBox("Cotizacion: ");
                if (Information.IsNumeric(cot))
                {
                    ibm.Cotizacion = decimal.Parse(cot);
                    dataGridView1_RowEnter(null, null);
                }
                else throw new Exception("debe ser un valor numerico!!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            
            
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            //Cuando entro a una fila
            //al trabajar con estado de grilla usar try con catch vacio
            try
            {
                //selecvcionamos lo que agarramos de la lista
                //solo podemos seleccionar una por eso el [0]
                //DataBoundItem es el objeto que se uso y el que se optuvieron los datos que se estan mostrando
                //en la grilla (que se que son inversionistas) asi q lo casteo como inversionista
                Inversionista iaux = (dataGridView1.SelectedRows[0].DataBoundItem as Inversionista);

                //ahora en la lista de ibm de inversionistas busco el dni buscado

                Inversionista iibm= ibm.RetornarInversionistas().Find(x => x.DNI == iaux.DNI);
                //var rdo = from x in iibm.RetornaCorizaciones() select new { x.FechaHora, x.Accion, x.Cotizacion };
                //ahora muestro
                //Mostrar(dataGridView2, rdo.ToArray());//esa lista de datos es la q veriamos ahi
                Mostrar(dataGridView2,iibm.RetornaCorizaciones());  
            }
            catch (Exception)
            {

            }
        }
    }
    public abstract class Accion
    {
        //posibilidad de subscribir inversores para avisarles que cambio la accion!!!!
        List<Inversionista> li;
        public Accion(string pDescripcion)
        {
            //acordarme que los constructores NO se heredan asi que en IBM tamb va
            li = new List<Inversionista>();
            Descripcion = pDescripcion;
        }
        public string Descripcion { get; set; }

        decimal cotizacion;

        public decimal Cotizacion
        {
            get { return cotizacion; } //el get y el set los hacemos explicito
            set
            {
                cotizacion = value;
                foreach (Inversionista i in li)//recorre sus subscriptores
                {
                    i.RecibeCotización(new Datos(DateTime.Now, this, value));
                    //le manda el mensaje al inversor
                }
            }
        }

        public void Subscribir(Inversionista pInversionista) //metodo que no devuelve nada y subscribe inversionista
        {

            try
            {
                Inversionista inv = li.Find(x => x.DNI == pInversionista.DNI);
                if (inv == null)//para todo inversionista x tal que el dni sea el que me pasaron por parametro. Si lo encuentra devuelve el inversionista
                {
                    //usamos mismo metodo de clonacio
                    li.Add(new Inversionista(pInversionista));
                }
                else { throw new Exception("El inversionista ya existe"); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
        public void QuitarSubscripcin(Inversionista pInversionista) //metodo que no devuelve nada y quita subscripcion de inversionista. 
        {
            try
            {
                Inversionista inv = li.Find(x => x.DNI == pInversionista.DNI);
                if (inv != null)//para todo inversionista x tal que el dni sea el que me pasaron por parametro. Si lo encuentra devuelve el inversionista
                {
                    li.Remove(inv);
                }
                else { throw new Exception("El inversionista no existe"); }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }


        }
        public List<Inversionista> RetornarInversionistas()
        {
            List<Inversionista> laux = new List<Inversionista>();
            //OJO HAY QUE CLONAR LOS DATOS DE LOS INVERSORES
            foreach (Inversionista i in li)
            {
                Inversionista iaux = new Inversionista(i);
                laux.Add(iaux);
                if (i.RetornaCorizaciones().Count > 0)
                {
                    foreach (Datos d in i.RetornaCorizaciones())
                    {
                        iaux.RetornaCorizaciones(d);
                    }
                }
            }
            return laux;
        }
        public override string ToString()
        {
            return Descripcion;
        }
    }
    public class IBM : Accion
    {
        public IBM(string pDescripcion) : base(pDescripcion)
        {
            //La sub clase debe impleentar como minimo mismo contructor que accion, llama al constructor de la clase base
        }
    }
    public class Inversionista
    {
        //La idea es que Inversionista publiqye un metodo (RecibeCotización)
        //que cuando el objeto accion le mande un mensaje, ese metodo pueda recepcionar
        //a travez de su parametro, por ejemplo la cotización a actual de la accion
        // y que haga algo con esa acción

        List<Datos> ld;


        public Inversionista(Inversionista pInversionista) : this(pInversionista.DNI, pInversionista.Nombre, pInversionista.Apellido)
        {
            //llama al otro constructor ya creado y le pasa a al parametro pfechahora lo que corta pInversionista.Apellido 
            //lo mismo para los otros parametros con esto logramos que puedan instanciar datos pasandole datos 
            //y que se inicialice
        }

        //Constructor para inicializar las propiedades 
        public Inversionista(string pDNI, string pNombre, string pApellido)
        {
            DNI = pDNI; Nombre = pNombre; Apellido = pApellido;
            ld = new List<Datos>();
        }
        public string DNI { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }

        public void RecibeCotización(Datos pDatos)
        {
            //si esto lo quiero para luego tener mas de una accion esta
            //bueno recibir por parametro que accion (Accion pAccion) es la que cambio de cotizacion
            //que la accion se mande ella misma cuando cambia su estado

            //RecibeCotizacion agarra la listra ld y le agrega una nueva
            //instancia de datos
            ld.Add(new Datos(pDatos)); //asi logro encapslar porque impido que la accion cambie el estado de un objeto que yo tengo en mi lista (rompimos con esta integridad referencial)
        }


        //este inversor quiero que me devuelva lo que tengo en la lista
        //por ejemplo en el formulario mostrarlo
        //por eso hago un metodo para retornar una lista de datos
        public List<Datos> RetornaCorizaciones()
        {
            //para enfatizar encapsulamiento:
            //  Si yo devolviera ld, el que recibe ld (ej formulario) podria 
            // hacer lo que quiera con ld (ej borrar entradas con remove) rompiendo el estado interno mio
            //por eso --> lo lógico seria retornar un clone de ld

            //forma de clonar a mano rapidamente (no es la mejor forma)
            List<Datos> laux = new List<Datos>();
            foreach (Datos d in ld)
            {
                laux.Add(new Datos(d));//le paso directamente un dato porque luego el constructor de Datos lo descompone para ver sus parametros y devolverme un nuevo objeto
            }
            return laux; //se lo pasa al formulario con la integridad intacta porque le di un clon
        }


        //destructor
        ~Inversionista()
        {
            ld = null;
        }


    }
    public class Datos
    {
        public Datos(Datos pDatos) : this(pDatos.FechaHora, pDatos.Accion, pDatos.Cotizacion)
        {
            //llama al otro constructor ya creado y le pasa a al parametro pfechahora lo que corta pDatos
            //en fecha y hora (pDatos.FechaHora) y lo mismo para los otros parametros
            //con esto logramos que puedan instanciar datos pasandole datos 
            //y que se inicialice
        }
        public Datos(DateTime pFechaHora, Accion pAccion, decimal pCotizacion)
        {
            FechaHora = pFechaHora;
            Accion = pAccion;
            Cotizacion = pCotizacion;
        }
        //Clase para ayudarme a almacenar datos de cotizcion en determinado momento
        public DateTime FechaHora { get; } //solo get porq solo lectura para poder solo cargarlo en el constructor
        public Accion Accion { get; } //propiedad que tiene la accion que cambio. Es de tipo accion

        public decimal Cotizacion { get; } //cuanto cotiza esa accion
    }
}
