using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PatronComposite
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }
        PermisoSimple ps1,ps2,ps3,ps4,ps5;

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Se ejecutó la funcoion 1");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Se ejecutó la funcoion 2");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Se ejecutó la funcoion 3");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Se ejecutó la funcoion 4");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Se ejecutó la funcoion 5");
        }

        private void Mostrar()
        {
            //Es la que define que se muestra en la interfaz segun el perfil del usuario
            int _y = 20;//la que uso para el espacio como acomodo los botones
            List<Button> _b = new List<Button>();
            foreach (Control c in this.Controls)//recorro coleccion de controles del form
            {
                if(c is Button)//pregunto si es boton
                {
                    (c as Button).Visible = false;//Lo primero que hago es ocultar los botones
                    if (_u.Perfil.Validar(c.Tag.ToString().Split(',')[0]))//si ese usuario en ese perfil valida,
                    {   //TENGO un tag en cada boton en sus propiedades, ej: boton 1 tiene 001,1
                        //por eso separa por la coma y solo se queda con la primer parte del tag 001
                        //ese 001 ese el codigo, ese codigo se usa como parametro en Validar porque validar
                        ////es buscar si ese codigo existe como codigo dentro del perfil
                        _b.Add(c as Button); //si tiene el permiso agrega el boton para que el usuario si pueda ver
                    }
                }
            }
            _b.Sort(new Orden()); //ordeno la lista de botones con SOrt y le mando una instancia de orden
            foreach (Control c in _b)
            {
                c.Visible = true;
                c.Top = _y; // lo pone en la posicion correspondiente ya que si oculto alguno quedan en cualquier lado del form. Los     
                _y += 55; 
                
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            _u.Perfil = _p01; Mostrar();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            _u.Perfil = _p02; Mostrar();
        }

        PermisoCompuesto pc1, pc2, pc3;
        Usuario _u;
        Perfil _p01, _p02;

        private void Form1_Load(object sender, EventArgs e)
        {
            //Tres situaciones
            //A) Compuestos que son solo perfiles
            //B) Compuestos que son solo intermedios
            //C) COmpuestos que pueden ser intermedios de otros compuestos y perfiles
            
            _u = new Usuario() { Nome = "Chiara", Apellido= "Digi"};

            ps1 = new PermisoSimple("001");
            ps2 = new PermisoSimple("002");
            ps3 = new PermisoSimple("003");
            ps4 = new PermisoSimple("004");
            ps5 = new PermisoSimple("005");

            pc1 = new PermisoCompuesto("C01");
            pc2 = new PermisoCompuesto("C02");
            pc3 = new PermisoCompuesto("C03");

            pc1.AgregarPermiso(ps1);
            pc1.AgregarPermiso(ps2);
            pc1.AgregarPermiso(ps3);
            pc1.AgregarPermiso(ps4);

            pc2.AgregarPermiso(pc1); 
            pc2.AgregarPermiso(ps5);

            pc3.AgregarPermiso(ps1);
            pc3.AgregarPermiso(ps4);
            pc3.AgregarPermiso(ps5);

            _p01 = new Perfil(pc2);
            _p02= new Perfil(pc3);

            _u.Perfil = _p01;
            Mostrar();
        }
    }

    public class Usuario
    {
        public string Nome { get; set; }
        public string Apellido { get; set; }
        public Perfil Perfil { get; set; }
    }
    public class Perfil
    {
        PermisoCompuesto _pc;//perfil de alguna manera va a estar representado x un permiso compuesto
        public Perfil(PermisoCompuesto pCompuesto) 
        { 
            _pc = pCompuesto; 
         }

        public bool Validar(string pCodigo)
        {
            //a este peril le pasan un codigo ( a donde el perfl quiere entrar)
            //y le devuelve si ese codigo esta o no permitido
            return _pc.RetornaPermiso().Exists(x => x.Codigo == pCodigo);
            //para cada x que retorna RetornaPermiso (lista de permisos simples) se fija 
            //si hay alguno que coincida (retorna verdadewr)
        }

    }
    public abstract class Permiso
    {
        //Clase abstracta que sería lo que el patrón plantea como Component
        
        string _codigo;

        public Permiso(string pCodigo)
        {
            //en este caso los permisos estan emulados por codigo
            _codigo = pCodigo;
        }
        public string Codigo
        {
            //propiedad de lectura y escritura para leer o escribir ese código
            get { return _codigo; }
            set { _codigo = value; }
        }
        //public bool SumaResta {get; set;{

        public abstract List<Permiso> RetornaPermiso();//lista de permisos que es un retorno de una funcion RetornaPermisos que va
                                                         //a implementar todas las sub clases (por eso es abstracta
        //Aca podria estar tambien lo de agregar y modificar
    }
    public class PermisoSimple : Permiso
    {
        public PermisoSimple(String pCodigo): base(pCodigo)//usa constructor de la super clase
        {

        }
        public override List<Permiso> RetornaPermiso()
        {
            return new List<Permiso>() { this };//este retorna una lista a la cual se agrega al mismo
            //no me queda otra que sea una lista por la restricción del polimorfismo que hereda de la super clase
        }
    }
    public class PermisoCompuesto : Permiso
    {
        List<Permiso> _l; //esta es la lista propia del patron, la que tiene
                            //los permisos que componen el permiso compuesto

        List<Permiso> _laux;

        public PermisoCompuesto(String pCodigo) : base(pCodigo)
        {
            //ademas de heredar constructor de clase base instancia lista de permisos
            _l = new List<Permiso>();
        }
        public void AgregarPermiso(Permiso Ppermiso)
        {
            _l.Add(Ppermiso);
        }
       public List<Permiso> Retornarcomponentes()
        {
            return _l;//este metodo es por algo funcional, no del patron
        }
        
        public override List<Permiso> RetornaPermiso()
        {
            //Aca esta la lógica propia del patron composite
            //esta recursiva
            _laux = new List<Permiso>();//instancio aca la lista auxiliar
            RecursivaRetornaPermisos(_l);//llamo a la recursiva y paso la lista original
            //esta recursiva lo que va a hcaer es cargarme la lista auxiar en sus componentes simples
            //y para que el metodo la retorne despes
            return _laux;
        }
        private void RecursivaRetornaPermisos(List<Permiso> pLista)
        {
            foreach( Permiso p in pLista)//iterator para cada permiso de la lista pregunta si es simpe
            {
                if(p is PermisoSimple)
                {
                    _laux.Add(p);//si es simple directamente lo agrega a la lista auxiliar
                }
                else
                {
                    //si es compuesto llama a la recursiva para que lo descomponga
                    RecursivaRetornaPermisos((p as PermisoCompuesto).Retornarcomponentes());
                                            //le digo a p q es permiso compuesto (lo casteo) y uso su retornacomponentes que da a lista l (original)
                }
            }
        }
    }
    public class Orden : IComparer<Button> //interfaz de comparación
    {
        public int Compare(Button x, Button y)//ventaja deesta interfaz es que yo decido el criterio de ordenamiento
        {
            //recibe dos objetos Button y tiene que devolver un valor < 0, 0, >0
            //si es menor q cero significa que el objeto x va a la izquierda del y,
            //si el valor es 0 significa que ambos objetos tienen la misma prioridad de ordenamiento
            //si el valor es mayor que 0 significa que y va antes que x
            int rdo = 0;
            if (int.Parse(x.Tag.ToString().Split(',')[1]) < int.Parse(y.Tag.ToString().Split(',')[1]))
            {
                //Con x.Tag.ToString().Split(',')[1] tomo el segundo elemento del vector tag que me dice la poscicion del boton.
                rdo = -1;

            }
            if (int.Parse(x.Tag.ToString().Split(',')[1]) > int.Parse(y.Tag.ToString().Split(',')[1]))
            {
                rdo = 1;
            }
            return rdo;


        }
    }
}
