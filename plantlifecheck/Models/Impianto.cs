using Microsoft.AspNetCore.Identity;
using Areas.Identity.Pages.Account;
using plantlifecheck.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Areas.User;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations.Schema;

namespace plantlifecheck.Models
{
    public class Impianto
    {
        public Impianto()
        {
            Macchine = new HashSet<Macchina>();
        }

        [Key]
        public long Id { get; set; }
        public CustomRole Nome { get; set; }
        public string Decrizione { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data ultima modifica", Prompt = "Data ultima modifica", Description = "Data ultima modifica")]
        public DateTime? UltimaModifica { get; set; } = DateTime.Now;
        [Display(Name = "Utente ultima modifica", Prompt = "Utente ultima modifica", Description = "Utente ultima modifica")]
        public CustomUser Utente { get; set; }

        public ICollection<Macchina> Macchine { get; set; }
        //public ICollection<ImpiantoMacchina> ImpiantiMacchine { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public class Macchina
    {
        public Macchina()
        {
            Componenti = new HashSet<Componente>();
        }

        [Key]
        public long Id { get; set; }
        public CustomRole Nome { get; set; }
        public string Decrizione { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data ultima modifica", Prompt = "Data ultima modifica", Description = "Data ultima modifica")]
        public DateTime? UltimaModifica { get; set; } = DateTime.Now;
        [Display(Name = "Utente ultima modifica", Prompt = "Utente ultima modifica", Description = "Utente ultima modifica")]
        public CustomUser Utente { get; set; }

        public Impianto Impianto { get; set; }
        public ICollection<Componente> Componenti { get; set; }
        //public ICollection<Organo> Organi { get; set; }
        //public ICollection<ImpiantoMacchina> ImpiantiMacchine { get; set; }
        //public ICollection<MacchinaOrgano> MacchineOrgani { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
    //public class Organo
    //{
    //    public Organo()
    //    {
    //        Componenti = new HashSet<Componente>();
    //    }

    //    [Key]
    //    public long Id { get; set; }
    //    public string Nome { get; set; }
    //    public string Decrizione { get; set; }

    //    public DateTime? UltimaModifica { get; set; }
    //    public CustomUser Utente { get; set; }

    //    public Macchina Macchina { get; set; }
    //    public ICollection<Componente> Componenti { get; set; }
    //    //public ICollection<MacchinaOrgano> MacchineOrgani { get; set; }
    //}

    public class Componente
    {
        
        public Componente()
        {
            ControlloVita = new HashSet<ControlloVita>();
        }

        [Key]
        public long Id { get; set; }
        public string Nome { get; set; }
        public string Decrizione { get; set; }
        public ICollection<ControlloVita> ControlloVita { get; set; }


        
        [DataType(DataType.Date)]
        [Display(Name = "Data ultima modifica", Prompt = "Data ultima modifica", Description = "Data ultima modifica")]
        public DateTime? UltimaModifica { get; set; } = DateTime.Now;
        [Display(Name = "Utente ultima modifica", Prompt = "Utente ultima modifica", Description = "Utente ultima modifica")]
        public CustomUser Utente { get; set; } 

        public Macchina Macchina { get; set; }
        public Impianto Impianto { get; set; }

        //public Organo Organo { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public class ControlloVita
    {
        [Key]
        public long Id { get; set; }

        private bool _Abilitato;
        [Display(Name = "Controllo abilitato", Prompt = "Controllo abilitato", Description = "Controllo abilitato")]
        public bool Abilitato
        {
            get
            {
                return _Abilitato;
            }
            set
            {
                if ((_Abilitato) && (!value)) Fermata = DateTime.Now;
                _Abilitato = value;
            }
        }
        [Required]
        [Display(Name = "Nome controllo", Prompt = "Nome controllo", Description = "Nome controllo")]
        public string Nome { get; set; }
        [Display(Name = "Descrizione controllo", Prompt = "Descrizione controllo", Description = "Descrizione controllo")]
        public string Descrizione { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Data inizio", Prompt = "Data inizio", Description = "Data inizio")]
        public DateTime? Partenza { get; set; }

        private DateTime? _Fermata;
        [DataType(DataType.Date)]
        [Display(Name = "Data fine", Prompt = "Data fine", Description = "Data fine")]
        public DateTime? Fermata
        {
            get
            {
                if (Abilitato) return DateTime.Now;
                return _Fermata;
            }
            set
            {
                _Fermata = value;
            }
        }

        [Display(Name = "Store Procedure abilitata (altrimenti differenza a tempo in giorni)", Prompt = "Store Procedure abilitata (altrimenti differenza a tempo in giorni)", Description = "Store Procedure abilitata  (altrimenti differenza a tempo in giorni)")]
        public bool SP_Consuntivo_Abilitato { get; set; } = false;
        [Display(Name = "Store Procedure selezionata", Prompt = "Store Procedure selezionata", Description = "Store Procedure selezionata")]
        public string SP_Consuntivo { get; set; }
        [Display(Name = "Valore consuntivo raggiunto", Prompt = "Valore consuntivo raggiunto", Description = "Valore consuntivo raggiunto")]
        public long? Consuntivo { get; set; } = 0;

        //public bool? Consuntivo_Abilitato { get; set; } = false;
        //public bool? Consuntivo_pausa { get; set; } = false;

        [DataType(DataType.Date)]
        [Display(Name = "Data ultimo aggiornamento consuntivo", Prompt = "Data ultimo aggiornamento consuntivo", Description = "Data ultimo aggiornamento consuntivo")]
        public DateTime? Ultimo_aggiornamento_Consuntivo { get; set; }

        [Display(Name = "Preallarme abilitato", Prompt = "Preallarme abilitato", Description = "Preallarme abilitato")]
        public bool preallarme_enable { get; set; } = false;
        [Display(Name = "Testo di preallarme", Prompt = "Testo di preallarme", Description = "Testo di preallarme")]
        public string preallarme_testo { get; set; } = "Raggiunta soglia di preallarme";
        [Display(Name = "Soglia di preallarme", Prompt = "Soglia di preallarme", Description = "Soglia di preallarme")]
        public long? preallarme_soglia { get; set; }
        [Display(Name = "Data intervento ultimo preallarme", Prompt = "Data intervento ultimo preallarme", Description = "Data intervento ultimo preallarme")]
        public DateTime? preallarme_data { get; set; }

        [Display(Name = "Allarme abilitato", Prompt = "Allarme abilitato", Description = "Allarme abilitato")]
        public bool allarme_enable { get; set; } = false;
        [Display(Name = "Testo di allarme", Prompt = "Testo di allarme", Description = "Testo di allarme")]
        public string allarme_testo { get; set; } = "Raggiunta soglia di allarme";
        [Display(Name = "Soglia di allarme", Prompt = "Soglia di allarme", Description = "Soglia di allarme")]
        public long? allarme_soglia { get; set; }
        [Display(Name = "Data intervento ultimo allarme", Prompt = "Data intervento ultimo allarme", Description = "Data intervento ultimo allarme")]
        public DateTime? allarme_data { get; set; }

        [NotMapped]
        public string DescrizioneCompleta
        {
            get
            {
                string result = "Nome controllo:" + (String.IsNullOrEmpty(Nome) ? "Null" : Nome) + Environment.NewLine;
                result = result + "Descrizione:" + (String.IsNullOrEmpty(Descrizione) ? "Null" : Descrizione);
                result = result + Environment.NewLine + "Componente:" + ((Componente==null)||(String.IsNullOrEmpty(Componente.Nome)) ? "Null" : Componente.Nome);
                result = result + Environment.NewLine + "Macchina:" + ((Macchina == null) || (Macchina.Nome == null) || (String.IsNullOrEmpty(Macchina.Nome.Name)) ? "Null" : Macchina.Nome.Name);
                result = result + Environment.NewLine + "Impianto:" + ((Impianto == null) || (Impianto.Nome == null) || (String.IsNullOrEmpty(Impianto.Nome.Name)) ? "Null" : Impianto.Nome.Name) + Environment.NewLine;
                return result;
            }
        }
        [NotMapped]
        public string DescrizioneBreve
        {
            get
            {
                string result = "Nome:" + (String.IsNullOrEmpty(Nome) ? "Null" : Nome) + "/";
                result = result + "Desc:" + (String.IsNullOrEmpty(Descrizione) ? "Null" : Descrizione);
                result = result + "/" + "Comp:" + ((Componente == null) || (String.IsNullOrEmpty(Componente.Nome)) ? "Null" : Componente.Nome);
                result = result + "/" + "Macc:" + ((Macchina == null) || (Macchina.Nome == null) || (String.IsNullOrEmpty(Macchina.Nome.Name)) ? "Null" : Macchina.Nome.Name);
                result = result + "/" + "Imp:" + ((Impianto == null) || (Impianto.Nome == null) || (String.IsNullOrEmpty(Impianto.Nome.Name)) ? "Null" : Impianto.Nome.Name) + "/";
                return result;
            }
        }




        [DataType(DataType.Date)]
        [Display(Name = "Data ultima modifica", Prompt = "Data ultima modifica", Description = "Data ultima modifica")]
        public DateTime? UltimaModifica { get; set; } = DateTime.Now;
        [Display(Name = "Utente ultima modifica", Prompt = "Utente ultima modifica", Description = "Utente ultima modifica")]
        public CustomUser Utente { get; set; }

        public Componente Componente { get; set; }
        public Macchina Macchina { get; set; }
        public Impianto Impianto { get; set; }

        [HiddenInput(DisplayValue = false)]
        [Timestamp]
        public byte[] RowVersion { get; set; }
    }





    //public  class ImpiantoMacchina
    //{
    //    public long ImpiantoId { get; set; }
    //    public long MacchinaId { get; set; }
    //    public Impianto Impianto { get; set; }
    //    public Macchina Macchina { get; set; }

    //    public DateTime AssociationDate { get; set; }
    //    public string User { get; set; }
    //}
    //public class MacchinaOrgano
    //{
    //    public long MacchinaId { get; set; }
    //    public long OrganoId { get; set; }
    //    public Impianto Macchina { get; set; }
    //    public Macchina Organo { get; set; }

    //    public DateTime AssociationDate { get; set; }
    //    public string User { get; set; }
    //}

}
