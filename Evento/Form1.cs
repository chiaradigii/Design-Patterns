using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace Evento
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //Boton crear inversionista
            ibm = new IBM("IBM");//este objeto es el que tiene el evento. Y estainstanciado en el objeto formulario.
                                 //entonces tengo que tener un metodo en el objeto formulario (porque es el que instancio a IBM)
                                 //que se subscriba al evento de IBM para que se entere
                                 //en este caso debo pensar, a donde quiero que llegue la información?
                                 //si yo quiero que se ejecute cierta funcion al realizar el evento (en nestro caso la funcion esta en inversionista)
                                 //Nosotros queremos que este en inversionista
                                 //tenemos una restriccion aca: cuando quiero subscribir un metodo tiene que responder a la firma del handler, este handler tiene firma object y CambioCotizacionEventArgs
                                 //        public void RecibeCotizacion(object sender, CambioCotizacionEventArgs e, Datos pDatos)

            ggal = new IBM("GGAL");
            i = new Inversionista("11.222.333", "JUan", "Perez");
            i2 = new Inversionista("42.647.8703", "Chiara", "Digiannantonio");

            ibm.CambioCotizacion += i.RecibeCotizacion;//subscribe un metodo de i (inversionista) que se llama RecibeCotizacion
            ibm.CambioCotizacion += i2.RecibeCotizacion;//subscribe un metodo de i (inversionista) que se llama RecibeCotizacion
            ggal.CambioCotizacion += i.RecibeCotizacion;//subscribe un metodo de i (inversionista) que se llama RecibeCotizacion

        }
        Inversionista i,i2;
        IBM ibm, ggal;

        private void button2_Click(object sender, EventArgs e)
        {
            //cuando hago click en cambiar cotizacion
            try
            {
                string cot = Interaction.InputBox("Cotizacion: ");
                if (Information.IsNumeric(cot))
                {
                    ggal.Cotizacion = decimal.Parse(cot);
                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = i.RetornaCorizaciones();
                    dataGridView2.DataSource = null;
                    dataGridView2.DataSource = i.RetornaCorizaciones();
                    //cada vez que cambio cotizacion cambia datagrid
                }
                else throw new Exception("debe ser un valor numerico!!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //cuando hago click en cambiar cotizacion
            try
            {
                string cot = Interaction.InputBox("Cotizacion: ");
                if (Information.IsNumeric(cot))
                {
                    ibm.Cotizacion = decimal.Parse(cot);
                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = i.RetornaCorizaciones();
                    dataGridView2.DataSource = null;
                    dataGridView2.DataSource = i.RetornaCorizaciones();
                    //cada vez que cambio cotizacion cambia datagrid
                }
                else throw new Exception("debe ser un valor numerico!!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }


        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }

    public class CambioCotizacionEventArgs : EventArgs
    {
        decimal cotizacion;

        public CambioCotizacionEventArgs(decimal pCotizacion, IBM pAccion)
        {
            Datos = new Datos(DateTime.Now, pAccion, pCotizacion);
        }
        //propiedad solo lecura para recumerar esa cotizacion
        public Datos Datos { get;}


    }
    
    public  class IBM
    {

        //declaramos el evento
        //lo hago geerico con un tipo personalizado. Argumento personalizado CambioCotizacionEventArgs
        public event EventHandler<CambioCotizacionEventArgs> CambioCotizacion;

        public IBM(string pDescripcion)
        {
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
                CambioCotizacion?.Invoke(this, new CambioCotizacionEventArgs(value,this));
            }
        }


        public override string ToString()
        {
            return Descripcion;
        }
    }

    public class Inversionista
    {
   
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



        public void RecibeCotizacion(object sender, CambioCotizacionEventArgs e)
        {

            ld.Add(new Datos(e.Datos)); //asi logro encapslar porque impido que la accion cambie el estado de un objeto que yo tengo en mi lista (rompimos con esta integridad referencial)
        }


              public List<Datos> RetornaCorizaciones()
        {
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
        public Datos(DateTime pFechaHora, IBM pAccion, decimal pCotizacion)
        {
            FechaHora = pFechaHora;
            Accion = pAccion;
            Cotizacion = pCotizacion;
        }
        //Clase para ayudarme a almacenar datos de cotizcion en determinado momento
        public DateTime FechaHora { get; } //solo get porq solo lectura para poder solo cargarlo en el constructor
        public IBM Accion { get; } //propiedad que tiene la accion que cambio. Es de tipo accion

        public decimal Cotizacion { get; } //cuanto cotiza esa accion
    }
}
