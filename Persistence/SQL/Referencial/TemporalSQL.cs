namespace Persistence.SQL.Referencial
{
    public class TemporalSQL
    {
        private string _query = "";
        public string SelectTemporal(string tablename)
        {
            switch (tablename)
            {
                case "tipo_identificacion":
                    _query = "select ti.codtipoidnt as Codigo, ti.numtipoidnt as Numero, ti.desctipoidnt as Descripcion from referential.tipo_identificacion ti ;";
                    break;
                case "ciudad":
                    _query = "select c.codciudad as Codigo, c.numciudad as Numero, c.descciudad as Descripcion from referential.ciudad c ;";
                    break;
                case "tipolistaprecio":
                    _query = "select tl.codlista as Codigo, tl.numlista as Numero, tl.deslista as Descripcion  from referential.tipolistaprecio tl ;";
                    break;
                case "familia":
                    _query = "select f.codfamilia as Codigo, f.numfamilia as Numero, f.desfamilia as Descripcion from referential.familia f ;";
                    break;
                case "rubro":
                    _query = "select r.codrubro as Codigo, r.numrubro as Numero, r.desrubro as Descripcion from referential.rubro r ;";
                    break;
                case "unidadmedida":
                    _query = "select u.codunidadmedida as Codigo, u.numunidadmedida as Numero, u.desunidadmedida as Descripcion from referential.unidadmedida u ;";
                    break;
                case "tipoiva":
                    _query = "select t.codiva as Codigo, t.numiva as Numero, t.desiva as Descripcion from referential.tipoiva t  ;";
                    break;
            }

            return _query;
        }
    }
}
