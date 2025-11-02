--
-- PostgreSQL database dump
--

-- Dumped from database version 16.4
-- Dumped by pg_dump version 16.4

-- Started on 2025-11-02 15:54:06

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 6 (class 2615 OID 231348)
-- Name: access; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA access;


ALTER SCHEMA access OWNER TO postgres;

--
-- TOC entry 7 (class 2615 OID 231349)
-- Name: purchase; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA purchase;


ALTER SCHEMA purchase OWNER TO postgres;

--
-- TOC entry 8 (class 2615 OID 231350)
-- Name: referential; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA referential;


ALTER SCHEMA referential OWNER TO postgres;

--
-- TOC entry 9 (class 2615 OID 231351)
-- Name: sales; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA sales;


ALTER SCHEMA sales OWNER TO postgres;

--
-- TOC entry 10 (class 2615 OID 231352)
-- Name: service; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA service;


ALTER SCHEMA service OWNER TO postgres;

--
-- TOC entry 11 (class 2615 OID 231353)
-- Name: shared; Type: SCHEMA; Schema: -; Owner: postgres
--

CREATE SCHEMA shared;


ALTER SCHEMA shared OWNER TO postgres;

--
-- TOC entry 297 (class 1255 OID 231354)
-- Name: fn_insert_compra(integer, text, timestamp without time zone, integer, integer, integer, text, text, integer, integer, integer, integer, integer, numeric, text, numeric, numeric, numeric, numeric, numeric, numeric, json); Type: FUNCTION; Schema: purchase; Owner: postgres
--

CREATE FUNCTION purchase.fn_insert_compra(p_codtipocomprobante integer, p_numcompra text, p_fechacompra timestamp without time zone, p_codproveedor integer, p_terminal integer, p_ultimo integer, p_finvalideztimbrado text, p_nrotimbrado text, p_codsucursal integer, p_codempleado integer, p_codestmov integer, p_condicionpago integer, p_codmoneda integer, p_cotizacion numeric, p_observacion text, p_totaliva numeric, p_totaldescuento numeric, p_totalexento numeric, p_totalgravada numeric, p_totalcompra numeric, p_codordenc numeric, p_detalles json) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codcompra INTEGER;
    detalle JSON;
    v_existe INTEGER;
BEGIN
    -- Validar si ya existe
    SELECT COUNT(*) INTO v_existe
    FROM purchase.compras
    WHERE codtipocomprobante = p_codtipocomprobante
      AND numcompra = p_numcompra and nrotimbrado = p_nrotimbrado and p_finvalideztimbrado = p_finvalideztimbrado
      AND codproveedor = p_codproveedor;

    IF v_existe > 0 THEN
        RAISE EXCEPTION 'La compra que esta intentando registrar ya existe';
    END IF;

    -- Obtener siguiente código
    SELECT COALESCE(MAX(codcompra), 0) + 1 INTO v_codcompra
    FROM purchase.compras;

    -- Insertar cabecera
    INSERT INTO purchase.compras (
        codcompra,
        codtipocomprobante,
		numcompra,
		fechacompra,
		codproveedor,
		finvalideztimbrado,
		nrotimbrado,
        codsucursal,
		codempleado,
        codestmov,
        condicionpago,
		codmoneda,
		cotizacion,
        observacion,
        totaliva,
        totaldescuento,
        totalexento,
        totalgravada,
        totalcompra,
		codordenc
    ) VALUES (
        v_codcompra,
        p_codtipocomprobante,
        p_numcompra,
        p_fechacompra,
        p_codproveedor,
		p_finvalideztimbrado,
		p_nrotimbrado,
		p_codsucursal,
		p_codempleado,
		p_codestmov,
		p_condicionpago,
		p_codmoneda,
		p_cotizacion,
		p_observacion,
		p_totaliva,
		p_totaldescuento,
		p_totalexento,
		p_totalgravada,
		p_totalcompra,
		p_codordenc
    );

    -- Insertar detalles
    FOR detalle IN
    SELECT elem
    FROM json_array_elements(p_detalles) AS elem
	LOOP
	    -- Insertar detalle de compra
	    INSERT INTO purchase.comprasdet (
	        codcompra, codproducto, coddepsuc, codiva, cantidad, descuento,
	        preciobruto, precioneto, cotizacion1, costoultimo
	    ) VALUES (
	        v_codcompra,
	        (detalle->>'codproducto')::INTEGER,
	        (detalle->>'coddepsuc')::INTEGER,
	        (detalle->>'codiva')::INTEGER,
	        (detalle->>'cantidad')::NUMERIC,
	        (detalle->>'descuento')::NUMERIC,
	        (detalle->>'preciobruto')::NUMERIC,
	        (detalle->>'precioneto')::NUMERIC,
	        (detalle->>'cotizacion1')::NUMERIC,
	        (detalle->>'costoultimo')::NUMERIC
	    );
	
	    -- Actualizar stock por producto/sucursal
	    UPDATE referential.productosucursal
	    SET cantidad = cantidad + (detalle->>'cantidad')::NUMERIC
	    WHERE codproducto = (detalle->>'codproducto')::INTEGER
	      AND codsucursal = (detalle->>'coddepsuc')::INTEGER;
	
	    -- Insertar si no existe
	    IF NOT FOUND THEN
	        INSERT INTO referential.productosucursal(
	            codproducto, codsucursal, cantidad
	        ) VALUES (
	            (detalle->>'codproducto')::INTEGER,
	            (detalle->>'coddepsuc')::INTEGER,
	            (detalle->>'cantidad')::NUMERIC
	        );
	    END IF;
	
	END LOOP;


    -- Actualizar comprobanteterminal
    UPDATE referential.comprobanteterminal
    SET actual = actual + 1
    WHERE codterminal = p_terminal
      AND codtipocomprobante = p_codtipocomprobante;

	if p_codordenc > 0 then
		update purchase.ordencompra set codestmov = 2 where codordenc = p_codordenc;
	end if;

    RETURN v_codcompra;
END;
$$;


ALTER FUNCTION purchase.fn_insert_compra(p_codtipocomprobante integer, p_numcompra text, p_fechacompra timestamp without time zone, p_codproveedor integer, p_terminal integer, p_ultimo integer, p_finvalideztimbrado text, p_nrotimbrado text, p_codsucursal integer, p_codempleado integer, p_codestmov integer, p_condicionpago integer, p_codmoneda integer, p_cotizacion numeric, p_observacion text, p_totaliva numeric, p_totaldescuento numeric, p_totalexento numeric, p_totalgravada numeric, p_totalcompra numeric, p_codordenc numeric, p_detalles json) OWNER TO postgres;

--
-- TOC entry 298 (class 1255 OID 231355)
-- Name: fn_insert_compra2(integer, text, timestamp without time zone, integer, integer, integer, text, text, integer, integer, integer, integer, integer, numeric, text, numeric, numeric, numeric, numeric, numeric, numeric, numeric, json); Type: FUNCTION; Schema: purchase; Owner: postgres
--

CREATE FUNCTION purchase.fn_insert_compra2(p_codtipocomprobante integer, p_numcompra text, p_fechacompra timestamp without time zone, p_codproveedor integer, p_terminal integer, p_ultimo integer, p_finvalideztimbrado text, p_nrotimbrado text, p_codsucursal integer, p_codempleado integer, p_codestmov integer, p_condicionpago integer, p_codmoneda integer, p_cotizacion numeric, p_observacion text, p_totaliva numeric, p_totaldescuento numeric, p_totalexento numeric, p_totalgravada numeric, p_totalcompra numeric, p_codordenc numeric, p_cantcuotas numeric, p_detalles json) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codcompra INTEGER;
    detalle JSON;
    v_existe INTEGER;
    v_montocuota integer;
    i integer;
BEGIN
    -- Validar si ya existe
    SELECT COUNT(*) INTO v_existe
    FROM purchase.compras
    WHERE codtipocomprobante = p_codtipocomprobante
      AND numcompra = p_numcompra and nrotimbrado = p_nrotimbrado and finvalideztimbrado  = p_finvalideztimbrado
      AND codproveedor = p_codproveedor;

    IF v_existe > 0 THEN
        RAISE EXCEPTION 'La compra que esta intentando registrar ya existe';
    END IF;

    -- Obtener siguiente código
    SELECT COALESCE(MAX(codcompra), 0) + 1 INTO v_codcompra
    FROM purchase.compras;

    -- Insertar cabecera
    INSERT INTO purchase.compras (
        codcompra,
        codtipocomprobante,
		numcompra,
		fechacompra,
		codproveedor,
		finvalideztimbrado,
		nrotimbrado,
        codsucursal,
		codempleado,
        codestmov,
        condicionpago,
		codmoneda,
		cotizacion,
        observacion,
        totaliva,
        totaldescuento,
        totalexento,
        totalgravada,
        totalcompra,
		codordenc
    ) VALUES (
        v_codcompra,
        p_codtipocomprobante,
        p_numcompra,
        p_fechacompra,
        p_codproveedor,
		p_finvalideztimbrado,
		p_nrotimbrado,
		p_codsucursal,
		p_codempleado,
		p_codestmov,
		p_condicionpago,
		p_codmoneda,
		p_cotizacion,
		p_observacion,
		p_totaliva,
		p_totaldescuento,
		p_totalexento,
		p_totalgravada,
		p_totalcompra,
		p_codordenc
    );

    -- Insertar detalles
    FOR detalle IN
    SELECT elem
    FROM json_array_elements(p_detalles) AS elem
	LOOP
	    -- Insertar detalle de compra
	    INSERT INTO purchase.comprasdet (
	        codcompra, codproducto, coddepsuc, codiva, cantidad, descuento,
	        preciobruto, precioneto, cotizacion1, costoultimo
	    ) VALUES (
	        v_codcompra,
	        (detalle->>'codproducto')::INTEGER,
	        (detalle->>'coddepsuc')::INTEGER,
	        (detalle->>'codiva')::INTEGER,
	        (detalle->>'cantidad')::NUMERIC,
	        (detalle->>'descuento')::NUMERIC,
	        (detalle->>'preciobruto')::NUMERIC,
	        (detalle->>'precioneto')::NUMERIC,
	        (detalle->>'cotizacion1')::NUMERIC,
	        (detalle->>'costoultimo')::NUMERIC
	    );
	
	    -- Actualizar stock por producto/sucursal
	    UPDATE referential.productosucursal
	    SET cantidad = cantidad + (detalle->>'cantidad')::NUMERIC
	    WHERE codproducto = (detalle->>'codproducto')::INTEGER
	      AND codsucursal = (detalle->>'coddepsuc')::INTEGER;
	
	    -- Insertar si no existe
	    IF NOT FOUND THEN
	        INSERT INTO referential.productosucursal(
	            codproducto, codsucursal, cantidad
	        ) VALUES (
	            (detalle->>'codproducto')::INTEGER,
	            (detalle->>'coddepsuc')::INTEGER,
	            (detalle->>'cantidad')::NUMERIC
	        );
	    END IF;
	
	END LOOP;

    -- Actualizar comprobanteterminal
    UPDATE referential.comprobanteterminal
    SET actual = actual + 1
    WHERE codterminal = p_terminal
      AND codtipocomprobante = p_codtipocomprobante;

	if p_codordenc > 0 then
		update purchase.ordencompra set codestmov = 2 where codordenc = p_codordenc;
	end if;

	if p_condicionpago = 1 then
		 v_montocuota := ROUND(p_totalcompra / p_cantcuotas, 0);
		 FOR i IN 1..p_cantcuotas LOOP
			INSERT INTO purchase.facturacompracredito (
            codcompra,
            nrocuota,
            montocuota,
            saldopendiente,
            fechavto
	        )
	        VALUES (
	            v_codcompra,
	            i,
	            v_montocuota,
	            v_montocuota,
	            current_date + (INTERVAL '1 month' * (i - 1))
	        );
		 End Loop;
	end if;

    RETURN v_codcompra;
END;
$$;


ALTER FUNCTION purchase.fn_insert_compra2(p_codtipocomprobante integer, p_numcompra text, p_fechacompra timestamp without time zone, p_codproveedor integer, p_terminal integer, p_ultimo integer, p_finvalideztimbrado text, p_nrotimbrado text, p_codsucursal integer, p_codempleado integer, p_codestmov integer, p_condicionpago integer, p_codmoneda integer, p_cotizacion numeric, p_observacion text, p_totaliva numeric, p_totaldescuento numeric, p_totalexento numeric, p_totalgravada numeric, p_totalcompra numeric, p_codordenc numeric, p_cantcuotas numeric, p_detalles json) OWNER TO postgres;

--
-- TOC entry 299 (class 1255 OID 231357)
-- Name: fn_insert_ordencompra(integer, integer, integer, text, timestamp without time zone, integer, integer, integer, integer, integer, numeric, numeric, numeric, numeric, numeric, numeric, text, integer, integer, json); Type: FUNCTION; Schema: purchase; Owner: postgres
--

CREATE FUNCTION purchase.fn_insert_ordencompra(p_codtipocomprobante integer, p_terminal integer, p_ultimo integer, p_numordencompra text, p_fechaorden timestamp without time zone, p_codestmov integer, p_codempleado integer, p_codproveedor integer, p_codmoneda integer, p_codsucursal integer, p_totaliva numeric, p_totaldescuento numeric, p_totalexento numeric, p_totalgravada numeric, p_totalordencompra numeric, p_cotizacion numeric, p_observacion text, p_condiciopago integer, p_codpresupuestocompra integer, p_detalles json) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codordenc INTEGER;
    detalle JSON;
    v_existe INTEGER;
BEGIN
    -- Validar si ya existe la orden de compra con el mismo tipo, número y sucursal
    SELECT COUNT(*) INTO v_existe
    FROM purchase.ordencompra
    WHERE codtipocomprobante = p_codtipocomprobante
      AND numordencompra = p_numordencompra
      AND codsucursal = p_codsucursal;

    IF v_existe > 0 THEN
        RAISE EXCEPTION 'Ya existe una orden de compra con ese tipo, número y sucursal';
    END IF;

    -- Obtener siguiente código para la orden de compra
    SELECT COALESCE(MAX(codordenc), 0) + 1 INTO v_codordenc
    FROM purchase.ordencompra;

    -- Insertar la cabecera de la orden de compra
    INSERT INTO purchase.ordencompra (
        codordenc,
        codtipocomprobante,
		codpresupuestocompra,
        codsucursal,
        fechaorden,
        numordencompra,
        codempleado,
        codestmov,
        condicionpago,
        codmoneda,
        cotizacion,
        codproveedor,
        observacion,
        totaliva,
        totaldescuento,
        totalexento,
        totalgravada,
        totalordencompra
    ) VALUES (
        v_codordenc,
        p_codtipocomprobante,
        p_codpresupuestocompra,
        p_codsucursal,
        p_fechaorden,
        p_numordencompra,
        p_codempleado,
        p_codestmov,
        p_condiciopago,
        p_codmoneda,
        p_cotizacion,
        p_codproveedor,
        p_observacion,
        p_totaliva,
        p_totaldescuento,
        p_totalexento,
        p_totalgravada,
        p_totalordencompra
    );

    -- Insertar los detalles de la orden de compra
    FOR detalle IN SELECT * FROM json_array_elements(p_detalles)
    LOOP
        INSERT INTO purchase.ordencompradet (
            codordenc,
            codproducto,
            codiva,
            cantidad,
            descuento,
            preciobruto,
            precioneto,
            cotizacion1
        ) VALUES (
            v_codordenc,
            (detalle->>'codproducto')::INTEGER,
            (detalle->>'codiva')::INTEGER,
            (detalle->>'cantidad')::NUMERIC,
            (detalle->>'descuento')::NUMERIC,
            (detalle->>'preciobruto')::NUMERIC,
            (detalle->>'precioneto')::NUMERIC,
            (detalle->>'cotizacion1')::NUMERIC
        );
    END LOOP;

    -- Actualizar el número de comprobante en el terminal
    UPDATE referential.comprobanteterminal
    SET actual = actual + 1
    WHERE codterminal = p_terminal
      AND codtipocomprobante = p_codtipocomprobante;

	update purchase.presupuestocompra set codestmov = 2 where codpresupuestocompra = p_codpresupuestocompra;

    -- Retornar el código de la orden de compra insertada
    RETURN v_codordenc;
END;
$$;


ALTER FUNCTION purchase.fn_insert_ordencompra(p_codtipocomprobante integer, p_terminal integer, p_ultimo integer, p_numordencompra text, p_fechaorden timestamp without time zone, p_codestmov integer, p_codempleado integer, p_codproveedor integer, p_codmoneda integer, p_codsucursal integer, p_totaliva numeric, p_totaldescuento numeric, p_totalexento numeric, p_totalgravada numeric, p_totalordencompra numeric, p_cotizacion numeric, p_observacion text, p_condiciopago integer, p_codpresupuestocompra integer, p_detalles json) OWNER TO postgres;

--
-- TOC entry 301 (class 1255 OID 231358)
-- Name: fn_insert_pedcompra(integer, text, timestamp without time zone, integer, integer, integer, json); Type: FUNCTION; Schema: purchase; Owner: postgres
--

CREATE FUNCTION purchase.fn_insert_pedcompra(p_codtipocomprobante integer, p_numpedcompra text, p_fechapedcompra timestamp without time zone, p_codestmov integer, p_codempleado integer, p_codsucursal integer, p_detalles json) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codpedcompra INTEGER;
    detalle JSON;
    v_existe INTEGER;
BEGIN

	-- Validar si ya existe un pedido con el mismo tipo, número y sucursal
    SELECT COUNT(*) INTO v_existe
    FROM purchase.pedidocompra
    WHERE codtipocomprobante = p_codtipocomprobante
      AND numpedcompra = p_numpedcompra
      AND codsucursal = p_codsucursal;

    IF v_existe > 0 THEN
        RAISE EXCEPTION 'Ya existe un pedido de compra con ese tipo, número y sucursal';
    END IF;

    -- Obtener el siguiente codpedcompra (último + 1)
    SELECT COALESCE(MAX(codpedcompra), 0) + 1 INTO v_codpedcompra
    FROM purchase.pedidocompra;
	
	
    -- Insertar cabecera
    INSERT INTO purchase.pedidocompra (
        codpedcompra, codtipocomprobante, numpedcompra,
        fechapedcompra, codestmov, codempleado, codsucursal
    ) VALUES (
        v_codpedcompra, p_codtipocomprobante, p_numpedcompra,
        p_fechapedcompra, p_codestmov, p_codempleado, p_codsucursal
    );

    -- Insertar cada detalle desde JSON
    FOR detalle IN SELECT * FROM json_array_elements(p_detalles)
    LOOP
        INSERT INTO purchase.pedidocompradet (
            codpedcompra, codproducto, cantidad, costoultimo
        ) VALUES (
            v_codpedcompra,
            (detalle->>'codproducto')::INTEGER,
            (detalle->>'cantidad')::NUMERIC,
            (detalle->>'costoultimo')::NUMERIC
        );
    END LOOP;

    RETURN v_codpedcompra;
END;
$$;


ALTER FUNCTION purchase.fn_insert_pedcompra(p_codtipocomprobante integer, p_numpedcompra text, p_fechapedcompra timestamp without time zone, p_codestmov integer, p_codempleado integer, p_codsucursal integer, p_detalles json) OWNER TO postgres;

--
-- TOC entry 302 (class 1255 OID 231359)
-- Name: fn_insert_pedcompra2(integer, integer, integer, text, timestamp without time zone, integer, integer, integer, json); Type: FUNCTION; Schema: purchase; Owner: postgres
--

CREATE FUNCTION purchase.fn_insert_pedcompra2(p_codtipocomprobante integer, p_terminal integer, p_ultimo integer, p_numpedcompra text, p_fechapedcompra timestamp without time zone, p_codestmov integer, p_codempleado integer, p_codsucursal integer, p_detalles json) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codpedcompra INTEGER;
    detalle JSON;
    v_existe INTEGER;
    v_ultimo integer;
BEGIN

	-- Validar si ya existe un pedido con el mismo tipo, número y sucursal
    SELECT COUNT(*) INTO v_existe
    FROM purchase.pedidocompra
    WHERE codtipocomprobante = p_codtipocomprobante
      AND numpedcompra = p_numpedcompra
      AND codsucursal = p_codsucursal;

    IF v_existe > 0 THEN
        RAISE EXCEPTION 'Ya existe un pedido de compra con ese tipo, número y sucursal';
    END IF;

    -- Obtener el siguiente codpedcompra (último + 1)
    SELECT COALESCE(MAX(codpedcompra), 0) + 1 INTO v_codpedcompra
    FROM purchase.pedidocompra;
	
	
    -- Insertar cabecera
    INSERT INTO purchase.pedidocompra (
        codpedcompra, codtipocomprobante, numpedcompra,
        fechapedcompra, codestmov, codempleado, codsucursal
    ) VALUES (
        v_codpedcompra, p_codtipocomprobante, p_numpedcompra,
        p_fechapedcompra, p_codestmov, p_codempleado, p_codsucursal
    );

    -- Insertar cada detalle desde JSON
    FOR detalle IN SELECT * FROM json_array_elements(p_detalles)
    LOOP
        INSERT INTO purchase.pedidocompradet (
            codpedcompra, codproducto, cantidad, costoultimo
        ) VALUES (
            v_codpedcompra,
            (detalle->>'codproducto')::INTEGER,
            (detalle->>'cantidad')::NUMERIC,
            (detalle->>'costoultimo')::NUMERIC
        );
    END LOOP;
	update referential.comprobanteterminal ct set actual = actual + 1 where ct.codterminal = p_terminal and ct.codtipocomprobante = p_codtipocomprobante;
    RETURN v_codpedcompra;
END;
$$;


ALTER FUNCTION purchase.fn_insert_pedcompra2(p_codtipocomprobante integer, p_terminal integer, p_ultimo integer, p_numpedcompra text, p_fechapedcompra timestamp without time zone, p_codestmov integer, p_codempleado integer, p_codsucursal integer, p_detalles json) OWNER TO postgres;

--
-- TOC entry 303 (class 1255 OID 231360)
-- Name: fn_insert_presupuestocompra(integer, integer, integer, text, timestamp without time zone, integer, integer, integer, integer, integer, numeric, numeric, numeric, numeric, numeric, numeric, text, text, integer, json); Type: FUNCTION; Schema: purchase; Owner: postgres
--

CREATE FUNCTION purchase.fn_insert_presupuestocompra(p_codtipocomprobante integer, p_terminal integer, p_ultimo integer, p_numpresupuestocompra text, p_fechapresupuesto timestamp without time zone, p_codestmov integer, p_codempleado integer, p_codproveedor integer, p_codmoneda integer, p_codsucursal integer, p_totaliva numeric, p_totaldescuento numeric, p_totalexento numeric, p_totalgravada numeric, p_totalpresupuestocompra numeric, p_cotizacion numeric, p_observacion text, p_contactoprv text, p_condiciopago integer, p_detalles json) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codpresupuestocompra INTEGER;
    detalle JSON;
    v_existe INTEGER;
BEGIN
    -- Validar si ya existe
    SELECT COUNT(*) INTO v_existe
    FROM purchase.presupuestocompra
    WHERE codtipocomprobante = p_codtipocomprobante
      AND numpresupuestocompra = p_numpresupuestocompra
      AND codsucursal = p_codsucursal;

    IF v_existe > 0 THEN
        RAISE EXCEPTION 'Ya existe un presupuesto compra con ese tipo, número y sucursal';
    END IF;

    -- Obtener siguiente código
    SELECT COALESCE(MAX(codpresupuestocompra), 0) + 1 INTO v_codpresupuestocompra
    FROM purchase.presupuestocompra;

    -- Insertar cabecera
    INSERT INTO purchase.presupuestocompra (
        codpresupuestocompra,
        codtipocomprobante,
        codsucursal,
        fechapresupuesto,
        numpresupuestocompra,
        codproveedor,
        contactoprv,
        condiciopago,
        codmoneda,
        totaliva,
        totaldescuento,
        totalexento,
        totalgravada,
        totalpresupuestocompra,
        cotizacion,
        codempleado,
        codestmov,
        observacion
    ) VALUES (
        v_codpresupuestocompra,
        p_codtipocomprobante,
        p_codsucursal,
        p_fechapresupuesto,
        p_numpresupuestocompra,
        p_codproveedor,
        p_contactoprv,
        p_condiciopago,
        p_codmoneda,
        p_totaliva,
        p_totaldescuento,
        p_totalexento,
        p_totalgravada,
        p_totalpresupuestocompra,
        p_cotizacion,
        p_codempleado,
        p_codestmov,
        p_observacion
    );

    -- Insertar detalles
    FOR detalle IN SELECT * FROM json_array_elements(p_detalles)
    LOOP
        INSERT INTO purchase.presupuestocompradet (
            codpresupuestocompra,
            codproducto,
            cantidad,
            preciobruto,
            precioneto,
            costoultimo,
            codiva
        ) VALUES (
            v_codpresupuestocompra,
            (detalle->>'codproducto')::INTEGER,
            (detalle->>'cantidad')::NUMERIC,
            (detalle->>'preciobruto')::NUMERIC,
            (detalle->>'precioneto')::NUMERIC,
            (detalle->>'costoultimo')::NUMERIC,
            (detalle->>'codiva')::INTEGER
        );
    END LOOP;

    -- Actualizar comprobanteterminal
    UPDATE referential.comprobanteterminal
    SET actual = actual + 1
    WHERE codterminal = p_terminal
      AND codtipocomprobante = p_codtipocomprobante;

    RETURN v_codpresupuestocompra;
END;
$$;


ALTER FUNCTION purchase.fn_insert_presupuestocompra(p_codtipocomprobante integer, p_terminal integer, p_ultimo integer, p_numpresupuestocompra text, p_fechapresupuesto timestamp without time zone, p_codestmov integer, p_codempleado integer, p_codproveedor integer, p_codmoneda integer, p_codsucursal integer, p_totaliva numeric, p_totaldescuento numeric, p_totalexento numeric, p_totalgravada numeric, p_totalpresupuestocompra numeric, p_cotizacion numeric, p_observacion text, p_contactoprv text, p_condiciopago integer, p_detalles json) OWNER TO postgres;

--
-- TOC entry 304 (class 1255 OID 231361)
-- Name: fn_update_compraestado(integer, integer); Type: FUNCTION; Schema: purchase; Owner: postgres
--

CREATE FUNCTION purchase.fn_update_compraestado(v_codcompra integer, p_codestmov integer) RETURNS text
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_ordenc integer;
	v_condicion integer;
	estado text;
    v_codestado integer;
begin

	SELECT codordenc INTO v_ordenc
    FROM purchase.compras
    WHERE codcompra = v_codcompra;

	-- Obtener el estado actual del pedido
    SELECT codestmov
    INTO v_codestado
    FROM purchase.compras
    WHERE codcompra = v_codcompra;

    IF v_codestado <> 1 THEN
        SELECT desestmov
        INTO estado
        FROM referential.estadomovimiento
        WHERE codestmov = v_codestado;

        RAISE EXCEPTION 'La compra no se puede anular, ya que cuenta con el estado: %', estado;
    END IF;

	IF v_ordenc > 0 THEN
        update purchase.ordencompra set codestmov = 1 where codordenc = v_ordenc ;
    END IF;

	SELECT condicionpago INTO v_condicion
    FROM purchase.compras where codcompra = v_codcompra;

	if v_condicion = 1 then
		delete from purchase.facturacompracredito where codcompra = v_codcompra;
	END IF;

	update purchase.compras set codestmov = p_codestmov, codordenc = NULL
    where codcompra = v_codcompra;

    RETURN 'OK: El estado de la compra fue actualizado correctamente';
EXCEPTION
    WHEN OTHERS THEN
        RETURN 'ERROR: ' || SQLERRM;
END;
$$;


ALTER FUNCTION purchase.fn_update_compraestado(v_codcompra integer, p_codestmov integer) OWNER TO postgres;

--
-- TOC entry 305 (class 1255 OID 231362)
-- Name: fn_update_ordencompraestado(integer, integer); Type: FUNCTION; Schema: purchase; Owner: postgres
--

CREATE FUNCTION purchase.fn_update_ordencompraestado(p_ordenc integer, p_codestmov integer) RETURNS text
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_ordenc integer;
    v_codpresupuesto integer;
	estado text;
    v_codestado integer;
begin

	-- Obtener el estado actual del pedido
    SELECT codestmov
    INTO v_codestado
    FROM purchase.ordencompra
    WHERE codordenc = p_ordenc;

    IF v_codestado <> 1 THEN
        SELECT desestmov
        INTO estado
        FROM referential.estadomovimiento
        WHERE codestmov = v_codestado;

        RAISE EXCEPTION 'La orden no se puede anular, ya que cuenta con el estado: %', estado;
    END IF;

	SELECT codordenc INTO v_ordenc
    FROM purchase.compras
    WHERE codordenc = p_ordenc;

	IF v_ordenc > 0 THEN
        RAISE EXCEPTION 'La orden no se puede anular ya se encuentra asociado a una Compra';
    END IF;

	SELECT codpresupuestocompra INTO v_codpresupuesto
    FROM purchase.ordencompra
    WHERE codordenc = v_ordenc;

	IF v_codpresupuesto > 0 THEN
        update purchase.presupuestocompra set codestmov = 1 where codpresupuestocompra = v_codpresupuesto ;
    END IF;

	update purchase.ordencompra set codestmov = p_codestmov, codpresupuestocompra = NULL
    where codordenc = p_ordenc;


    RETURN 'OK: El estado de la orden fue actualizado correctamente';
EXCEPTION
    WHEN OTHERS THEN
        RETURN 'ERROR: ' || SQLERRM;
END;
$$;


ALTER FUNCTION purchase.fn_update_ordencompraestado(p_ordenc integer, p_codestmov integer) OWNER TO postgres;

--
-- TOC entry 285 (class 1255 OID 231363)
-- Name: fn_update_pedidocompradet(integer, json); Type: FUNCTION; Schema: purchase; Owner: postgres
--

CREATE FUNCTION purchase.fn_update_pedidocompradet(p_codpedcompra integer, p_newdetalle json) RETURNS text
    LANGUAGE plpgsql
    AS $$
DECLARE
    detalle JSON;
BEGIN
    DELETE FROM purchase.pedidocompradet
    WHERE codpedcompra = p_codpedcompra;
    FOR detalle IN SELECT * FROM json_array_elements(p_newdetalle)
    LOOP
        INSERT INTO purchase.pedidocompradet (
            codpedcompra,
            codproducto,
            cantidad,
            costoultimo
        ) VALUES (
            p_codpedcompra,
            (detalle->>'codproducto')::INTEGER,
            (detalle->>'cantidad')::NUMERIC,
            (detalle->>'costoultimo')::NUMERIC
        );
    END LOOP;
    RETURN 'OK: Detalles actualizados correctamente';
EXCEPTION
    WHEN OTHERS THEN
        RETURN 'ERROR: ' || SQLERRM;
END;
$$;


ALTER FUNCTION purchase.fn_update_pedidocompradet(p_codpedcompra integer, p_newdetalle json) OWNER TO postgres;

--
-- TOC entry 300 (class 1255 OID 231364)
-- Name: fn_update_pedidocompraestado(integer, integer); Type: FUNCTION; Schema: purchase; Owner: postgres
--

CREATE FUNCTION purchase.fn_update_pedidocompraestado(p_codpedcompra integer, p_codestmov integer) RETURNS text
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codpedido integer;
    mensaje text;
    estado text;
    v_codestado integer;
BEGIN
    -- Verificar si el pedido ya está asociado a un presupuesto
    SELECT codpedcompra
    INTO v_codpedido
    FROM purchase.presupuestocompra
    WHERE codpedcompra = p_codpedcompra;

    IF v_codpedido IS NOT NULL THEN
        RAISE EXCEPTION 'El pedido no se puede anular, ya se encuentra asociado a un Presupuesto';
    END IF;

    -- Obtener el estado actual del pedido
    SELECT codestmov
    INTO v_codestado
    FROM purchase.pedidocompra
    WHERE codpedcompra = p_codpedcompra;

    IF v_codestado <> 1 THEN
        SELECT desestmov
        INTO estado
        FROM referential.estadomovimiento
        WHERE codestmov = v_codestado;

        RAISE EXCEPTION 'El pedido no se puede anular, ya que cuenta con el estado: %', estado;
    END IF;

    -- Actualizar estado del pedido (si es necesario)
    UPDATE purchase.pedidocompra
    SET codestmov = p_codestmov
    WHERE codpedcompra = p_codpedcompra;

    RETURN 'OK: El estado del pedido fue actualizado correctamente';

EXCEPTION
    WHEN OTHERS THEN
        RETURN 'ERROR: ' || SQLERRM;
END;
$$;


ALTER FUNCTION purchase.fn_update_pedidocompraestado(p_codpedcompra integer, p_codestmov integer) OWNER TO postgres;

--
-- TOC entry 306 (class 1255 OID 231365)
-- Name: fn_update_presupuestocompraestado(integer, integer); Type: FUNCTION; Schema: purchase; Owner: postgres
--

CREATE FUNCTION purchase.fn_update_presupuestocompraestado(p_codpresupuestocompra integer, p_codestmov integer) RETURNS text
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codpedido integer;
    v_codpresupuesto integer;
	estado text;
    v_codestado integer;
begin

	-- Obtener el estado actual del pedido
    SELECT codestmov
    INTO v_codestado
    FROM purchase.presupuestocompra
    WHERE codpresupuestocompra = p_codpresupuestocompra;

    IF v_codestado <> 1 THEN
        SELECT desestmov
        INTO estado
        FROM referential.estadomovimiento
        WHERE codestmov = v_codestado;

        RAISE EXCEPTION 'El presupuesto no se puede anular, ya que cuenta con el estado: %', estado;
    END IF;

	SELECT codpresupuestocompra compra INTO v_codpresupuesto
    FROM purchase.ordencompra
    WHERE codpresupuestocompra = p_codpresupuestocompra;

	IF v_codpresupuesto > 0 THEN
        RAISE EXCEPTION 'El presupuesto no se puede anular ya se encuentra asociado a una Orden';
    END IF;

	SELECT codpedcompra INTO v_codpedido
    FROM purchase.presupuestocompra
    WHERE codpresupuestocompra = p_codpresupuestocompra;

	IF v_codpedido > 0 THEN
        update purchase.pedidocompra set codestmov = 1 where codpedcompra = v_codpedido ;
    END IF;

	update purchase.presupuestocompra set codestmov = p_codestmov, codpedcompra = NULL
    where codpresupuestocompra = p_codpresupuestocompra;


    RETURN 'OK: El estado del presupuesto fue actualizado correctamente';
EXCEPTION
    WHEN OTHERS THEN
        RETURN 'ERROR: ' || SQLERRM;
END;
$$;


ALTER FUNCTION purchase.fn_update_presupuestocompraestado(p_codpresupuestocompra integer, p_codestmov integer) OWNER TO postgres;

--
-- TOC entry 321 (class 1255 OID 255924)
-- Name: fn_apertura_caja(integer, integer, timestamp with time zone, numeric, integer); Type: FUNCTION; Schema: referential; Owner: postgres
--

CREATE FUNCTION referential.fn_apertura_caja(p_codcaja integer, p_codcobrador integer, p_fechaapertura timestamp with time zone, p_montoapertura numeric, p_codterminal integer) RETURNS json
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_estado_caja RECORD;
    v_caja_terminal RECORD;
    v_caja_otra_terminal RECORD;
    v_last RECORD;
    v_next_cod INT;
BEGIN

    SELECT * INTO v_estado_caja
    FROM referential.cajagestion
    WHERE codcaja = p_codcaja AND estado = FALSE
    LIMIT 1;

    IF FOUND THEN
        RAISE EXCEPTION 'La caja ya está aperturada';
    END IF;

    SELECT * INTO v_caja_terminal
    FROM referential.cajagestion
    WHERE codterminal = p_codterminal AND estado = FALSE
    LIMIT 1;

    IF FOUND THEN
        RAISE EXCEPTION 'La terminal solo puede tener una caja abierta';
    END IF;

    SELECT * INTO v_caja_otra_terminal
    FROM referential.cajagestion
    WHERE codcaja = p_codcaja
      AND codterminal <> p_codterminal
      AND estado = FALSE
    LIMIT 1;

    IF FOUND THEN
        RAISE EXCEPTION 'La caja ya está abierta en otra terminal';
    END IF;

    SELECT COALESCE(MAX(codgestion), 0) + 1 INTO v_next_cod
    FROM referential.cajagestion;

    INSERT INTO referential.cajagestion (
        codgestion,
        codcaja,
        codcobrador,
        fechaapertura,
        montoapertura,
        estado,
        codterminal
    )
    VALUES (
        v_next_cod,
        p_codcaja,
        p_codcobrador,
        p_fechaapertura,
        p_montoapertura,
        FALSE,
        p_codterminal
    );

    RETURN json_build_object(
        'message', 'Apertura Realizada',
        'gestion', v_next_cod,
        'cobrador', p_codcobrador,
        'estadocaja', FALSE
    );

EXCEPTION
    WHEN OTHERS THEN
        RETURN json_build_object(
            'error', SQLERRM
        );
END;
$$;


ALTER FUNCTION referential.fn_apertura_caja(p_codcaja integer, p_codcobrador integer, p_fechaapertura timestamp with time zone, p_montoapertura numeric, p_codterminal integer) OWNER TO postgres;

--
-- TOC entry 319 (class 1255 OID 255925)
-- Name: fn_cierre_caja(integer, timestamp with time zone, numeric); Type: FUNCTION; Schema: referential; Owner: postgres
--

CREATE FUNCTION referential.fn_cierre_caja(p_codgestion integer, p_fechacierre timestamp with time zone, p_montocierre numeric) RETURNS json
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_caja RECORD;
BEGIN
    SELECT *
    INTO v_caja
    FROM referential.cajagestion
    WHERE codgestion = p_codgestion;

    IF v_caja IS NULL THEN
        RETURN json_build_object(
            'error', TRUE,
            'message', 'No se encontró la gestión especificada'
        );
    END IF;

    IF v_caja.estado = TRUE THEN
        RETURN json_build_object(
            'error', TRUE,
            'message', 'La caja ya está cerrada'
        );
    END IF;

    UPDATE referential.cajagestion
    SET fechacierre = p_fechacierre,
        montocierre = p_montocierre,
        estado = TRUE
    WHERE codgestion = p_codgestion;

    RETURN json_build_object(
        'error', FALSE,
        'message', 'Cierre Realizado',
        'gestion', p_codgestion,
        'estadocaja', TRUE
    );
END;
$$;


ALTER FUNCTION referential.fn_cierre_caja(p_codgestion integer, p_fechacierre timestamp with time zone, p_montocierre numeric) OWNER TO postgres;

--
-- TOC entry 307 (class 1255 OID 231366)
-- Name: fn_insert_pedventa(integer, integer, integer, timestamp without time zone, text, integer, integer, integer, numeric, numeric, integer, integer, json); Type: FUNCTION; Schema: sales; Owner: postgres
--

CREATE FUNCTION sales.fn_insert_pedventa(p_codtipocomprobante integer, p_codsucursal integer, p_codestmov integer, p_fechapedidov timestamp without time zone, p_numpedventa text, p_codvendedor integer, p_codcliente integer, p_codmoneda integer, p_totalpedidov numeric, p_cotizacion1 numeric, p_ultimo integer, p_codterminal integer, p_detalles json) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codpedidov INTEGER;
    detalle JSON;
BEGIN
    -- Obtener el siguiente codpedcompra (último + 1)
    SELECT COALESCE(MAX(codpedidov), 0) + 1 INTO v_codpedidov
    FROM sales.pedidoventa;

    -- Insertar cabecera
    INSERT INTO sales.pedidoventa (
        codpedidov, codtipocomprobante, codsucursal,
        codestmov, fechapedidov , numpedventa, codvendedor, codcliente, codmoneda, totalpedidov, cotizacion1
    ) VALUES (
        v_codpedidov ,p_codtipocomprobante, p_codsucursal,
		p_codestmov, p_fechapedidov, p_numpedventa, p_codvendedor, p_codcliente, p_codmoneda, p_totalpedidov, p_cotizacion1
    );

    -- Insertar cada detalle desde JSON
    FOR detalle IN SELECT * FROM json_array_elements(p_detalles)
    LOOP
        INSERT INTO sales.pedidoventadet (
            codpedidov, codproducto, cantidad, precioventa
        ) VALUES (
            v_codpedidov,
            (detalle->>'codproducto')::INTEGER,
            (detalle->>'cantidad')::NUMERIC,
            (detalle->>'precioventa')::NUMERIC
        );
    END LOOP;

	UPDATE referential.comprobanteterminal ct
    	SET actual = actual + 1
    WHERE ct.codterminal = p_codterminal
      AND ct.codtipocomprobante = p_codtipocomprobante;

    RETURN v_codpedidov;
END;
$$;


ALTER FUNCTION sales.fn_insert_pedventa(p_codtipocomprobante integer, p_codsucursal integer, p_codestmov integer, p_fechapedidov timestamp without time zone, p_numpedventa text, p_codvendedor integer, p_codcliente integer, p_codmoneda integer, p_totalpedidov numeric, p_cotizacion1 numeric, p_ultimo integer, p_codterminal integer, p_detalles json) OWNER TO postgres;

--
-- TOC entry 308 (class 1255 OID 231367)
-- Name: fn_insert_presupuestoventa(integer, integer, integer, integer, timestamp without time zone, text, integer, text, integer, smallint, integer, numeric, integer, numeric, numeric, numeric, numeric, numeric, integer, integer, json); Type: FUNCTION; Schema: sales; Owner: postgres
--

CREATE FUNCTION sales.fn_insert_presupuestoventa(p_codtipocomprobante integer, p_codsucursal integer, p_codvendedor integer, p_codcliente integer, p_fechapresupuesto timestamp without time zone, p_numprstventa text, p_codpedidov integer, p_observacion text, p_diaven integer, p_condicionpago smallint, p_codmoneda integer, p_cotizacion numeric, p_codestmov integer, p_totaliva numeric, p_totaldescuento numeric, p_totalexento numeric, p_totalgravada numeric, p_totalpresupuestoventa numeric, p_terminal integer, p_ultimo integer, p_detalles json) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codpresupuestoventa INTEGER;
    detalle JSON;
    v_existe INTEGER;
BEGIN
    -- Validar si ya existe un presupuesto con mismo tipo, número y sucursal
    SELECT COUNT(*) INTO v_existe
    FROM sales.presupuestoventa
    WHERE codtipocomprobante = p_codtipocomprobante
      AND numpresupuestoventa = p_numprstventa
      AND codsucursal = p_codsucursal;

    IF v_existe > 0 THEN
        RAISE EXCEPTION 'Ya existe un presupuesto de venta con ese tipo, número y sucursal';
    END IF;

    -- Obtener siguiente código para cabecera
    SELECT COALESCE(MAX(codpresupuestoventa), 0) + 1
    INTO v_codpresupuestoventa
    FROM sales.presupuestoventa;

    -- Insertar cabecera
    INSERT INTO sales.presupuestoventa (
        codpresupuestoventa,
        codtipocomprobante,
        codsucursal,
        codvendedor,
        codcliente,
        fechapresupuestoventa,
        numpresupuestoventa,
        codpedidov,
        observacion,
        diaven,
        condicionpago,
        codmoneda,
        cotizacion1,
        codestmov,
        totaliva,
        totaldescuento,
        totalexento,
        totalgravada,
        totalpresupuestoventa
    ) VALUES (
        v_codpresupuestoventa,
        p_codtipocomprobante,
        p_codsucursal,
        p_codvendedor,
        p_codcliente,
        p_fechapresupuesto,
        p_numprstventa,
        p_codpedidov,
        p_observacion,
        p_diaven,
        p_condicionpago,
        p_codmoneda,
        p_cotizacion,
        p_codestmov,
        p_totaliva,
        p_totaldescuento,
        p_totalexento,
        p_totalgravada,
        p_totalpresupuestoventa
    );

    -- Insertar detalles (JSON -> filas)
    FOR detalle IN SELECT * FROM json_array_elements(p_detalles)
    LOOP
        INSERT INTO sales.presupuestoventadet (
            codpresupuestoventa,
            codproducto,
            precioneto,
            preciobruto,
            cantidad,
            costoultimo,
            codiva
        ) VALUES (
            v_codpresupuestoventa,
            (detalle->>'codproducto')::INTEGER,
            (detalle->>'precioneto')::NUMERIC,
            (detalle->>'preciobruto')::NUMERIC,
            (detalle->>'cantidad')::NUMERIC,
            (detalle->>'costoultimo')::NUMERIC,
            (detalle->>'codiva')::INTEGER
        );
    END LOOP;

    -- Actualizar secuencia de comprobante
    UPDATE referential.comprobanteterminal
    SET actual = actual + 1
    WHERE codterminal = p_terminal
      AND codtipocomprobante = p_codtipocomprobante;

    RETURN v_codpresupuestoventa;
END;
$$;


ALTER FUNCTION sales.fn_insert_presupuestoventa(p_codtipocomprobante integer, p_codsucursal integer, p_codvendedor integer, p_codcliente integer, p_fechapresupuesto timestamp without time zone, p_numprstventa text, p_codpedidov integer, p_observacion text, p_diaven integer, p_condicionpago smallint, p_codmoneda integer, p_cotizacion numeric, p_codestmov integer, p_totaliva numeric, p_totaldescuento numeric, p_totalexento numeric, p_totalgravada numeric, p_totalpresupuestoventa numeric, p_terminal integer, p_ultimo integer, p_detalles json) OWNER TO postgres;

--
-- TOC entry 309 (class 1255 OID 231368)
-- Name: fn_insert_ventas(integer, text, timestamp without time zone, integer, integer, integer, text, text, integer, integer, integer, integer, integer, numeric, text, numeric, numeric, numeric, numeric, numeric, numeric, numeric, json); Type: FUNCTION; Schema: sales; Owner: postgres
--

CREATE FUNCTION sales.fn_insert_ventas(p_codtipocomprobante integer, p_numventa text, p_fechaventa timestamp without time zone, p_codcliente integer, p_terminal integer, p_ultimo integer, p_finvalideztimbrado text, p_nrotimbrado text, p_codsucursal integer, p_codvendedor integer, p_codestmov integer, p_condicionpago integer, p_codmoneda integer, p_cotizacion numeric, p_observacion text, p_totaliva numeric, p_totaldescuento numeric, p_totalexento numeric, p_totalgravada numeric, p_totalventa numeric, p_codpresupuestoventa numeric, p_cantcuotas numeric, p_detalles json) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codventa INTEGER;
    detalle JSON;
    v_existe INTEGER;
    v_montocuota NUMERIC;
    i INTEGER;
BEGIN
    -- Validar si ya existe
    SELECT COUNT(*) INTO v_existe
    FROM sales.ventas
    WHERE codtipocomprobante = p_codtipocomprobante
      AND numventa = p_numventa
      AND nrotimbrado = p_nrotimbrado
      AND finvalideztimbrado = p_finvalideztimbrado
      AND codcliente = p_codcliente;

    IF v_existe > 0 THEN
        RAISE EXCEPTION 'Ya existe una venta con los datos registrados';
    END IF;

    -- Obtener siguiente código
    SELECT COALESCE(MAX(codventa), 0) + 1 INTO v_codventa
    FROM sales.ventas;

    -- Insertar cabecera
    INSERT INTO sales.ventas (
        codventa, codtipocomprobante, numventa, fechaventa, codcliente,
        finvalideztimbrado, nrotimbrado, codsucursal, codvendedor, codestmov,
        condicionpago, codmoneda, cotizacion, observacion,
        totaliva, totaldescuento, totalexento, totalgravada, totalventa, codpresupuestoventa
    )
    VALUES (
        v_codventa, p_codtipocomprobante, p_numventa, p_fechaventa, p_codcliente,
        p_finvalideztimbrado, p_nrotimbrado, p_codsucursal, p_codvendedor, p_codestmov,
        p_condicionpago, p_codmoneda, p_cotizacion, p_observacion,
        p_totaliva, p_totaldescuento, p_totalexento, p_totalgravada, p_totalventa, p_codpresupuestoventa
    );

    -- Insertar detalles
    FOR detalle IN
        SELECT elem
        FROM json_array_elements(p_detalles) AS elem
    LOOP
        INSERT INTO sales.ventasdet (
            codventa, codproducto, codiva, cantidad, descuento,
            preciobruto, precioneto, cotizacion1, costoultimo
        )
        VALUES (
            v_codventa,
            (detalle->>'codproducto')::INTEGER,
            (detalle->>'codiva')::INTEGER,
            (detalle->>'cantidad')::NUMERIC,
            (detalle->>'descuento')::NUMERIC,
            (detalle->>'preciobruto')::NUMERIC,
            (detalle->>'precioneto')::NUMERIC,
            (detalle->>'cotizacion1')::NUMERIC,
            (detalle->>'costoultimo')::NUMERIC
        );

        -- Actualizar stock
        UPDATE referential.productosucursal
        SET cantidad = cantidad - (detalle->>'cantidad')::NUMERIC
        WHERE codproducto = (detalle->>'codproducto')::INTEGER
          AND codsucursal = p_codsucursal;

        -- Si no existe, insertar
        IF NOT FOUND THEN
            RAISE EXCEPTION 'El producto no tiene stock o asignado un precio';
        END IF;
    END LOOP;

    -- Actualizar comprobante terminal
    UPDATE referential.comprobanteterminal
    SET actual = actual + 1
    WHERE codterminal = p_terminal
      AND codtipocomprobante = p_codtipocomprobante;

    -- Actualizar presupuesto si corresponde
    IF p_codpresupuestoventa > 0 THEN
        UPDATE sales.presupuestoventa
        SET codestmov = 2
        WHERE codpresupuestoventa = p_codpresupuestoventa;
    END IF;

    -- Generar cuotas si es crédito
    IF p_condicionpago = 1 THEN
        v_montocuota := ROUND(p_totalventa / p_cantcuotas, 0);

        FOR i IN 1..p_cantcuotas LOOP
            INSERT INTO sales.facturaventacredito (
                codventa, nrocuota, montocuota, saldopendiente, fechavto
            )
            VALUES (
                v_codventa,
                i,
                v_montocuota,
                v_montocuota,
                current_date + (INTERVAL '1 month' * (i - 1))
            );
        END LOOP;
    END IF;

    RETURN v_codventa;
END;
$$;


ALTER FUNCTION sales.fn_insert_ventas(p_codtipocomprobante integer, p_numventa text, p_fechaventa timestamp without time zone, p_codcliente integer, p_terminal integer, p_ultimo integer, p_finvalideztimbrado text, p_nrotimbrado text, p_codsucursal integer, p_codvendedor integer, p_codestmov integer, p_condicionpago integer, p_codmoneda integer, p_cotizacion numeric, p_observacion text, p_totaliva numeric, p_totaldescuento numeric, p_totalexento numeric, p_totalgravada numeric, p_totalventa numeric, p_codpresupuestoventa numeric, p_cantcuotas numeric, p_detalles json) OWNER TO postgres;

--
-- TOC entry 317 (class 1255 OID 247731)
-- Name: fn_update_pedidoventaestado(integer, integer); Type: FUNCTION; Schema: sales; Owner: postgres
--

CREATE FUNCTION sales.fn_update_pedidoventaestado(p_codpedidov integer, p_codestmov integer) RETURNS text
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codpedido integer;
    mensaje text;
    estado text;
    v_codestado integer;
BEGIN
    -- Verificar si el pedido ya está asociado a un presupuesto
    SELECT codpedidov
    INTO v_codpedido
    FROM sales.presupuestoventa
    WHERE codpedidov = p_codpedidov;

	if v_codpedido is null then
		RAISE EXCEPTION 'El pedido no existe';
    END IF;

    IF v_codpedido IS NOT NULL THEN
        RAISE EXCEPTION 'El pedido no se puede anular, ya se encuentra asociado a un Presupuesto';
    END IF;

    -- Obtener el estado actual del pedido
    SELECT codestmov
    INTO v_codestado
    FROM sales.pedidoventa
    WHERE codpedidov = p_codpedidov;

    IF v_codestado <> 1 THEN
        SELECT desestmov
        INTO estado
        FROM referential.estadomovimiento
        WHERE codestmov = v_codestado;

        RAISE EXCEPTION 'El pedido no se puede anular, ya que cuenta con el estado: %', estado;
    END IF;

    -- Actualizar estado del pedido (si es necesario)
    UPDATE sales.pedidoventa
    SET codestmov = p_codestmov
    WHERE codpedidov = p_codpedidov;

    RETURN 'OK: El estado del pedido fue actualizado correctamente';

EXCEPTION
    WHEN OTHERS THEN
        RETURN 'ERROR: ' || SQLERRM;
END;
$$;


ALTER FUNCTION sales.fn_update_pedidoventaestado(p_codpedidov integer, p_codestmov integer) OWNER TO postgres;

--
-- TOC entry 318 (class 1255 OID 247732)
-- Name: fn_update_presupuestoventaestado(integer, integer); Type: FUNCTION; Schema: sales; Owner: postgres
--

CREATE FUNCTION sales.fn_update_presupuestoventaestado(p_codpresupuestoventa integer, p_codestmov integer) RETURNS text
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codpedido integer;
    v_codpresupuesto integer;
	estado text;
    v_codestado integer;
begin

	SELECT codpresupuestoventa
    INTO v_codpresupuesto
    FROM sales.presupuestoventa
    WHERE codpresupuestoventa = p_codpresupuestoventa;

	if v_codpresupuesto is null then
		RAISE EXCEPTION 'El presupuesto no existe';
    END IF;

	-- Obtener el estado actual del pedido
    SELECT codestmov
    INTO v_codestado
    FROM sales.presupuestoventa
    WHERE codpresupuestoventa = p_codpresupuestoventa;

    IF v_codestado <> 1 THEN
        SELECT desestmov
        INTO estado
        FROM referential.estadomovimiento
        WHERE codestmov = v_codestado;

        RAISE EXCEPTION 'El presupuesto no se puede anular, ya que cuenta con el estado: %', estado;
    END IF;

	SELECT codpresupuestoventa INTO v_codpresupuesto
    FROM sales.ventas
    WHERE codpresupuestoventa = p_codpresupuestoventa;

	IF v_codpresupuesto > 0 THEN
        RAISE EXCEPTION 'El presupuesto no se puede anular ya se encuentra asociado a una Venta';
    END IF;

	SELECT codpedidov INTO v_codpedido
    FROM sales.presupuestoventa
    WHERE codpresupuestoventa = p_codpresupuestoventa;

	IF v_codpedido > 0 THEN
        update sales.pedidoventa set codestmov = 1 where codpedidov = v_codpedido ;
    END IF;

	update sales.presupuestoventa set codestmov = p_codestmov, codpedidov = NULL
    where codpresupuestoventa = p_codpresupuestoventa;


    RETURN 'OK: El estado del presupuesto fue actualizado correctamente';
EXCEPTION
    WHEN OTHERS THEN
        RETURN 'ERROR: ' || SQLERRM;
END;
$$;


ALTER FUNCTION sales.fn_update_presupuestoventaestado(p_codpresupuestoventa integer, p_codestmov integer) OWNER TO postgres;

--
-- TOC entry 320 (class 1255 OID 231369)
-- Name: fn_update_ventaestado(integer, integer); Type: FUNCTION; Schema: sales; Owner: postgres
--

CREATE FUNCTION sales.fn_update_ventaestado(v_codventa integer, p_codestmov integer) RETURNS text
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_presupuesto integer;
	v_venta integer;
	v_condicion integer;
	estado text;
    v_codestado integer;
begin

	SELECT codventa
    	INTO v_venta
    FROM sales.ventas
    WHERE codventa = v_codventa;

	if v_venta is null then
		RAISE EXCEPTION 'La venta no existe';
    END IF;

	select * 
		into v_venta
	from shared.notacredito
	where codventa = v_codventa;
	
	if v_venta is null then
		RAISE EXCEPTION 'La venta ya esta asociada a una NOTA DE CREDITO';
    END IF;	

	SELECT codpresupuestoventa INTO v_presupuesto
    FROM sales.ventas
    WHERE codventa = v_codventa;

	-- Obtener el estado actual del pedido
    SELECT codestmov
    INTO v_codestado
    FROM sales.ventas
    WHERE codventa = v_codventa;

    IF v_codestado <> 1 THEN
        SELECT desestmov
        INTO estado
        FROM referential.estadomovimiento
        WHERE codestmov = v_codestado;

        RAISE EXCEPTION 'La venta no se puede anular, ya que cuenta con el estado: %', estado;
    END IF;

	IF v_presupuesto > 0 THEN
        update sales.presupuestoventa set codestmov = 1 where codpresupuestoventa = v_presupuesto ;
    END IF;

	SELECT condicionpago INTO v_condicion
    FROM sales.ventas where codventa = v_codventa;

	if v_condicion = 1 then
		delete from sales.facturaventacredito where codventa = v_codventa;
	END IF;

	update sales.ventas set codestmov = p_codestmov, codpresupuestoventa = NULL
    where codventa = v_codventa;

    RETURN 'OK: El estado de la venta fue actualizado correctamente';
EXCEPTION
    WHEN OTHERS THEN
        RETURN 'ERROR: ' || SQLERRM;
END;
$$;


ALTER FUNCTION sales.fn_update_ventaestado(v_codventa integer, p_codestmov integer) OWNER TO postgres;

--
-- TOC entry 310 (class 1255 OID 231370)
-- Name: fn_insert_diagnosticotecnico(integer, integer, text, integer, integer, timestamp without time zone, integer, integer, integer, json); Type: FUNCTION; Schema: service; Owner: postgres
--

CREATE FUNCTION service.fn_insert_diagnosticotecnico(p_codtipocomprobante integer, p_codsucursal integer, p_nrodiagnostico text, p_codestmov integer, p_codempleado integer, p_fechadiagnostico timestamp without time zone, p_codvehiculo integer, p_terminal integer, p_ultimo integer, p_detalles json) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_coddiagnostico INTEGER;
    detalle JSON;
    v_existe INTEGER;
BEGIN
    -- Validar si ya existe e diagnostico con el mismo tipo, número y sucursal
	IF EXISTS(SELECT 1 FROM service.diagnosticotecnico
        WHERE codtipocomprobante = p_codtipocomprobante
          AND nrodiagnostico = p_nrodiagnostico
          AND codsucursal = p_codsucursal
    ) THEN
        RAISE EXCEPTION 'Ya existe un registro con ese tipo, número y sucursal';
    END IF;

    -- Obtener siguiente código para la orden de compra
    SELECT COALESCE(MAX(coddiagnostico), 0) + 1 INTO v_coddiagnostico
    FROM service.diagnosticotecnico;

    -- Insertar la cabecera de la orden de compra
    INSERT INTO service.diagnosticotecnico(coddiagnostico, codtipocomprobante, codsucursal, nrodiagnostico, codestmov, codempleado,
    fechadiagnostico, codvehiculo) VALUES (v_coddiagnostico, p_codtipocomprobante, p_codsucursal, p_nrodiagnostico, p_codestmov, p_codempleado,
    p_fechadiagnostico, p_codvehiculo);

    -- Insertar los detalles de la orden de compra
    FOR detalle IN SELECT * FROM json_array_elements(p_detalles)
    LOOP
        INSERT INTO service.diagnosticotecnicodet (
            coddiagnostico, codparte, observacion
        ) VALUES (
            v_coddiagnostico,
            (detalle->>'codparte')::INTEGER,
            (detalle->>'observacion')::TEXT
        );
    END LOOP;

    -- Actualizar el número de comprobante en el terminal
    UPDATE referential.comprobanteterminal
    SET actual = actual + 1
    WHERE codterminal = p_terminal
      AND codtipocomprobante = p_codtipocomprobante;

    -- Retornar el código de la orden de compra insertada
    RETURN v_coddiagnostico;
END;
$$;


ALTER FUNCTION service.fn_insert_diagnosticotecnico(p_codtipocomprobante integer, p_codsucursal integer, p_nrodiagnostico text, p_codestmov integer, p_codempleado integer, p_fechadiagnostico timestamp without time zone, p_codvehiculo integer, p_terminal integer, p_ultimo integer, p_detalles json) OWNER TO postgres;

--
-- TOC entry 311 (class 1255 OID 231371)
-- Name: fn_insert_registrovehiculo(integer, integer, integer, integer, integer, text, timestamp without time zone, integer, text, text, text, integer); Type: FUNCTION; Schema: service; Owner: postgres
--

CREATE FUNCTION service.fn_insert_registrovehiculo(p_codcliente integer, p_codsucursal integer, p_codempleado integer, p_codestmov integer, p_codtipocomprobante integer, p_numregistro text, p_fecharegistro timestamp without time zone, p_codmarca integer, p_modelo text, p_nrochapa text, p_nrochasis text, p_codterminal integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codregistro INTEGER;
    v_existe INTEGER;
BEGIN
    -- Validar si ya existe un registro con el mismo tipo, número y sucursal
    SELECT COUNT(*) INTO v_existe
    FROM service.registrovehiculo
    WHERE codtipocomprobante = p_codtipocomprobante
      AND numregistro = p_numregistro
      AND codsucursal = p_codsucursal;

    IF v_existe > 0 THEN
        RAISE EXCEPTION 'Ya existe un registro con ese tipo, número y sucursal';
    END IF;

    -- Obtener el siguiente código
    SELECT COALESCE(MAX(codregistro), 0) + 1 INTO v_codregistro
    FROM service.registrovehiculo;

    -- Insertar cabecera
    INSERT INTO service.registrovehiculo (
        codregistro, codcliente, codsucursal, codempleado, codestmov, codtipocomprobante,
        numregistro, fecharegistro, codmarca, modelo, nrochapa, nrochasis
    ) VALUES (
        v_codregistro, p_codcliente, p_codsucursal, p_codempleado, p_codestmov, p_codtipocomprobante,
        p_numregistro, p_fecharegistro, p_codmarca, p_modelo, p_nrochapa, p_nrochasis
    );

    -- Actualizar número actual de comprobante en la terminal
    UPDATE referential.comprobanteterminal ct
    SET actual = actual + 1
    WHERE ct.codterminal = p_codterminal
      AND ct.codtipocomprobante = p_codtipocomprobante;

    RETURN v_codregistro;
END;
$$;


ALTER FUNCTION service.fn_insert_registrovehiculo(p_codcliente integer, p_codsucursal integer, p_codempleado integer, p_codestmov integer, p_codtipocomprobante integer, p_numregistro text, p_fecharegistro timestamp without time zone, p_codmarca integer, p_modelo text, p_nrochapa text, p_nrochasis text, p_codterminal integer) OWNER TO postgres;

--
-- TOC entry 312 (class 1255 OID 231372)
-- Name: fn_insert_registrovehiculo2(integer, integer, integer, integer, integer, text, timestamp without time zone, integer, text, text, text, integer); Type: FUNCTION; Schema: service; Owner: postgres
--

CREATE FUNCTION service.fn_insert_registrovehiculo2(p_codcliente integer, p_codsucursal integer, p_codempleado integer, p_codestmov integer, p_codtipocomprobante integer, p_numregistro text, p_fecharegistro timestamp without time zone, p_codmarca integer, p_modelo text, p_nrochapa text, p_nrochasis text, p_codterminal integer) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codregistro INTEGER;
	v_codvehiculo INTEGER;
    v_existe INTEGER;
	v_existevehiculo INTEGER;
BEGIN

	SELECT codvehiculo INTO v_codvehiculo
    FROM referential.vehiculo
    WHERE codcliente = p_codcliente
      AND nrochapa = p_nrochapa;

	 IF v_codvehiculo IS NULL THEN
		
		SELECT COALESCE(MAX(codvehiculo), 0) + 1 INTO v_codvehiculo
        FROM referential.vehiculo;

		INSERT INTO referential.vehiculo (
            codvehiculo, codcliente, nrochapa, modelo, codmarca, nrochasis
        ) VALUES (
            v_codvehiculo, p_codcliente, p_nrochapa, p_modelo, p_codmarca, p_nrochasis
        );
        
    END IF;

	
    -- Validar si ya existe un registro con el mismo tipo, número y sucursal
    SELECT COUNT(*) INTO v_existe
    FROM service.registrovehiculo
    WHERE codtipocomprobante = p_codtipocomprobante
      AND numregistro = p_numregistro
      AND codsucursal = p_codsucursal;

    IF v_existe > 0 THEN
        RAISE EXCEPTION 'Ya existe un registro con ese tipo, número y sucursal';
    END IF;

    -- Obtener el siguiente código
    SELECT COALESCE(MAX(codregistro), 0) + 1 INTO v_codregistro
    FROM service.registrovehiculo;

    -- Insertar cabecera
    INSERT INTO service.registrovehiculo (
        codregistro, codcliente, codsucursal, codempleado, codtipocomprobante,
        numregistro, fecharegistro, codvehiculo
    ) VALUES (
        v_codregistro, p_codcliente, p_codsucursal, p_codempleado, p_codtipocomprobante,
        p_numregistro, p_fecharegistro, v_codvehiculo
    );

    -- Actualizar número actual de comprobante en la terminal
    UPDATE referential.comprobanteterminal ct
    SET actual = actual + 1
    WHERE ct.codterminal = p_codterminal
      AND ct.codtipocomprobante = p_codtipocomprobante;

    RETURN v_codregistro;
END;
$$;


ALTER FUNCTION service.fn_insert_registrovehiculo2(p_codcliente integer, p_codsucursal integer, p_codempleado integer, p_codestmov integer, p_codtipocomprobante integer, p_numregistro text, p_fecharegistro timestamp without time zone, p_codmarca integer, p_modelo text, p_nrochapa text, p_nrochasis text, p_codterminal integer) OWNER TO postgres;

--
-- TOC entry 313 (class 1255 OID 231373)
-- Name: fn_insert_ajustes(integer, integer, text, timestamp without time zone, integer, integer, integer, integer, integer, json); Type: FUNCTION; Schema: shared; Owner: postgres
--

CREATE FUNCTION shared.fn_insert_ajustes(p_codtipocomprobante integer, p_codsucursal integer, p_numajuste text, p_fechaajuste timestamp without time zone, p_codmotivo integer, p_codempleado integer, p_condicion integer, p_codterminal integer, p_ultimo integer, p_detalles json) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codajuste INTEGER;
    v_existe INTEGER;
	v_producto text;
    detalle JSON;
BEGIN
    SELECT COUNT(*) INTO v_existe
    FROM shared.ajustes
    WHERE codtipocomprobante = p_codtipocomprobante
      AND numajuste = p_numajuste
      AND codsucursal = p_codsucursal;

    IF v_existe > 0 THEN
        RAISE EXCEPTION 'Ya existe un ajuste con ese tipo, número y sucursal';
    END IF;

    SELECT COALESCE(MAX(codajuste), 0) + 1 INTO v_codajuste
    FROM shared.ajustes;

    INSERT INTO shared.ajustes (
        codajuste,
        codtipocomprobante,
        codsucursal,
        numajuste,
        fechaajuste,
        codmotivo,
        codempleado,
        condicion
    ) VALUES (
        v_codajuste,
        p_codtipocomprobante,
        p_codsucursal,
        p_numajuste,
        p_fechaajuste,
        p_codmotivo,
        p_codempleado,
        p_condicion
    );

    FOR detalle IN SELECT * FROM json_array_elements(p_detalles)
	LOOP
	    -- Validar stock solo si la operación es de resta (ej: condicion = 2)
	    IF p_condicion = 1 THEN
	        PERFORM 1
	        FROM referential.productosucursal
	        WHERE codproducto = (detalle->>'codproducto')::INTEGER
	          AND codsucursal = p_codsucursal
	          AND cantidad >= (detalle->>'cantidad')::NUMERIC;
	
	        IF NOT FOUND THEN
				select codigobarra || ' ' || desproducto into v_producto 
				from referential.producto where codproducto = (detalle->>'codproducto')::INTEGER;
	            RAISE EXCEPTION 'Stock insuficiente para el producto %',
	                v_producto;
	        END IF;
	    END IF;
	
	    -- Insertar en ajustesdet
	    INSERT INTO shared.ajustesdet (
	        codajuste,
	        codproducto,
	        cantidad
	    ) VALUES (
	        v_codajuste,
	        (detalle->>'codproducto')::INTEGER,
	        (detalle->>'cantidad')::NUMERIC
	    );
	
	    -- Actualizar stock según condición
	    IF p_condicion = 0 THEN
	        -- Sumar stock
	        UPDATE referential.productosucursal
	        SET cantidad = cantidad + (detalle->>'cantidad')::NUMERIC
	        WHERE codproducto = (detalle->>'codproducto')::INTEGER
	          AND codsucursal = p_codsucursal;
	
	        IF NOT FOUND THEN
	            INSERT INTO referential.productosucursal(
	                codproducto, codsucursal, cantidad
	            ) VALUES (
	                (detalle->>'codproducto')::INTEGER,
	                p_codsucursal,
	                (detalle->>'cantidad')::NUMERIC
	            );
	        END IF;
	    ELSE
	        -- Restar stock
	        UPDATE referential.productosucursal
	        SET cantidad = cantidad - (detalle->>'cantidad')::NUMERIC
	        WHERE codproducto = (detalle->>'codproducto')::INTEGER
	          AND codsucursal = p_codsucursal;
	    END IF;
	END LOOP;


    UPDATE referential.comprobanteterminal
    SET actual = actual + 1
    WHERE codterminal = p_codterminal
      AND codtipocomprobante = p_codtipocomprobante;

    RETURN v_codajuste;
END;
$$;


ALTER FUNCTION shared.fn_insert_ajustes(p_codtipocomprobante integer, p_codsucursal integer, p_numajuste text, p_fechaajuste timestamp without time zone, p_codmotivo integer, p_codempleado integer, p_condicion integer, p_codterminal integer, p_ultimo integer, p_detalles json) OWNER TO postgres;

--
-- TOC entry 314 (class 1255 OID 231374)
-- Name: fn_insert_notacredito_compra(integer, integer, integer, text, timestamp without time zone, text, date, integer, integer, integer, integer, numeric, numeric, numeric, numeric, numeric, numeric, integer, integer, json); Type: FUNCTION; Schema: shared; Owner: postgres
--

CREATE FUNCTION shared.fn_insert_notacredito_compra(p_codcompra integer, p_codproveedor integer, p_codtipocomprobante integer, p_numnotacredito text, p_fechanotacredito timestamp without time zone, p_nrotimbrado text, p_fechavalidez date, p_codestmov integer, p_codempleado integer, p_codsucursal integer, p_codmoneda integer, p_cotizacion numeric, p_totaliva numeric, p_totalexenta numeric, p_totalgravada numeric, p_totaldescuento numeric, p_totaldevolucion numeric, p_terminal integer, p_ultimo integer, p_detalles json) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codnotacredito INTEGER;
    v_existe INTEGER;
    detalle JSON;
BEGIN
    -- Validar si ya existe una nota de crédito con el mismo tipo, número, sucursal y timbrado
    SELECT COUNT(*) INTO v_existe
    FROM shared.notacredito
    WHERE codtipocomprobante = p_codtipocomprobante
      AND numnotacredito = p_numnotacredito
      AND codsucursal = p_codsucursal
      AND nrotimbrado = p_nrotimbrado
      AND fechavalidez = p_fechavalidez;

    IF v_existe > 0 THEN
        RAISE EXCEPTION 'Ya existe una Nota de Crédito con el mismo tipo, número, sucursal y timbrado';
    END IF;

    -- Obtener el siguiente código de nota de crédito
    SELECT COALESCE(MAX(codnotacredito), 0) + 1 INTO v_codnotacredito
    FROM shared.notacredito;
    -- Insertar cabecera
    INSERT INTO shared.notacredito (
        codnotacredito, codcompra, codproveedor, codtipocomprobante,
        numnotacredito, nrotimbrado, fechavalidez, fechanotacredito,
        codsucursal, codempleado, codestmov, codmoneda, cotizacion,
        totaliva, totalexenta, totalgravada, totaldescuento, totaldevolucion, movimiento
    ) VALUES (
        v_codnotacredito, p_codcompra, p_codproveedor, p_codtipocomprobante,
        p_numnotacredito, p_nrotimbrado, p_fechavalidez, p_fechanotacredito,
        p_codsucursal, p_codempleado, p_codestmov, p_codmoneda, p_cotizacion,
        p_totaliva, p_totalexenta, p_totalgravada, p_totaldescuento, p_totaldevolucion, 'COMPRAS'
    );

    -- Insertar detalles desde JSON
    FOR detalle IN SELECT * FROM json_array_elements(p_detalles)
    LOOP
        INSERT INTO shared.notacreditodet (
            codnotacredito,
            codproducto,
            cantidaddev,
            preciobruto,
            precioneto,
            costoultimo,
            codiva
        ) VALUES (
            v_codnotacredito,
            (detalle->>'codproducto')::INTEGER,
            (detalle->>'cantidaddev')::NUMERIC,
            (detalle->>'preciobruto')::NUMERIC,
            (detalle->>'precioneto')::NUMERIC,
            (detalle->>'costoultimo')::NUMERIC,
            (detalle->>'codiva')::INTEGER
        );
    END LOOP;

	UPDATE referential.comprobanteterminal
    SET actual = actual + 1
    WHERE codterminal = p_terminal
      AND codtipocomprobante = p_codtipocomprobante;

    RETURN v_codnotacredito;
END;
$$;


ALTER FUNCTION shared.fn_insert_notacredito_compra(p_codcompra integer, p_codproveedor integer, p_codtipocomprobante integer, p_numnotacredito text, p_fechanotacredito timestamp without time zone, p_nrotimbrado text, p_fechavalidez date, p_codestmov integer, p_codempleado integer, p_codsucursal integer, p_codmoneda integer, p_cotizacion numeric, p_totaliva numeric, p_totalexenta numeric, p_totalgravada numeric, p_totaldescuento numeric, p_totaldevolucion numeric, p_terminal integer, p_ultimo integer, p_detalles json) OWNER TO postgres;

--
-- TOC entry 315 (class 1255 OID 231375)
-- Name: fn_insert_notacredito_venta(integer, integer, integer, text, timestamp without time zone, text, date, integer, integer, integer, integer, numeric, numeric, numeric, numeric, numeric, numeric, integer, integer, json); Type: FUNCTION; Schema: shared; Owner: postgres
--

CREATE FUNCTION shared.fn_insert_notacredito_venta(p_codventa integer, p_codcliente integer, p_codtipocomprobante integer, p_numnotacredito text, p_fechanotacredito timestamp without time zone, p_nrotimbrado text, p_fechavalidez date, p_codestmov integer, p_codempleado integer, p_codsucursal integer, p_codmoneda integer, p_cotizacion numeric, p_totaliva numeric, p_totalexenta numeric, p_totalgravada numeric, p_totaldescuento numeric, p_totaldevolucion numeric, p_terminal integer, p_ultimo integer, p_detalles json) RETURNS integer
    LANGUAGE plpgsql
    AS $$
DECLARE
    v_codnotacredito INTEGER;
    v_existe INTEGER;
    detalle JSON;
BEGIN
    -- Validar si ya existe una nota de crédito con el mismo tipo, número, sucursal y timbrado
    SELECT COUNT(*) INTO v_existe
    FROM shared.notacredito
    WHERE codtipocomprobante = p_codtipocomprobante
      AND numnotacredito = p_numnotacredito
      AND codsucursal = p_codsucursal
      AND nrotimbrado = p_nrotimbrado
      AND fechavalidez = p_fechavalidez;

    IF v_existe > 0 THEN
        RAISE EXCEPTION 'Ya existe una Nota de Crédito con el mismo tipo, número, sucursal y timbrado';
    END IF;

    -- Obtener el siguiente código de nota de crédito
    SELECT COALESCE(MAX(codnotacredito), 0) + 1 INTO v_codnotacredito
    FROM shared.notacredito;
    -- Insertar cabecera
    INSERT INTO shared.notacredito (
        codnotacredito, codventa, codcliente, codtipocomprobante,
        numnotacredito, nrotimbrado, fechavalidez, fechanotacredito,
        codsucursal, codempleado, codestmov, codmoneda, cotizacion,
        totaliva, totalexenta, totalgravada, totaldescuento, totaldevolucion, movimiento
    ) VALUES (
        v_codnotacredito, p_codventa, p_codcliente, p_codtipocomprobante,
        p_numnotacredito, p_nrotimbrado, p_fechavalidez, p_fechanotacredito,
        p_codsucursal, p_codempleado, p_codestmov, p_codmoneda, p_cotizacion,
        p_totaliva, p_totalexenta, p_totalgravada, p_totaldescuento, p_totaldevolucion, 'VENTAS'
    );

    -- Insertar detalles desde JSON
    FOR detalle IN SELECT * FROM json_array_elements(p_detalles)
    LOOP
        INSERT INTO shared.notacreditodet (
            codnotacredito,
            codproducto,
            cantidaddev,
            preciobruto,
            precioneto,
            costoultimo,
            codiva
        ) VALUES (
            v_codnotacredito,
            (detalle->>'codproducto')::INTEGER,
            (detalle->>'cantidaddev')::NUMERIC,
            (detalle->>'preciobruto')::NUMERIC,
            (detalle->>'precioneto')::NUMERIC,
            (detalle->>'costoultimo')::NUMERIC,
            (detalle->>'codiva')::INTEGER
        );
    END LOOP;

	UPDATE referential.comprobanteterminal
    SET actual = actual + 1
    WHERE codterminal = p_terminal
      AND codtipocomprobante = p_codtipocomprobante;

    RETURN v_codnotacredito;
END;
$$;


ALTER FUNCTION shared.fn_insert_notacredito_venta(p_codventa integer, p_codcliente integer, p_codtipocomprobante integer, p_numnotacredito text, p_fechanotacredito timestamp without time zone, p_nrotimbrado text, p_fechavalidez date, p_codestmov integer, p_codempleado integer, p_codsucursal integer, p_codmoneda integer, p_cotizacion numeric, p_totaliva numeric, p_totalexenta numeric, p_totalgravada numeric, p_totaldescuento numeric, p_totaldevolucion numeric, p_terminal integer, p_ultimo integer, p_detalles json) OWNER TO postgres;

--
-- TOC entry 316 (class 1255 OID 231376)
-- Name: fn_notacredito_list_detalle(text, integer); Type: FUNCTION; Schema: shared; Owner: postgres
--

CREATE FUNCTION shared.fn_notacredito_list_detalle(p_tipo text, p_codmovimiento integer) RETURNS TABLE(codproducto integer, datoproducto text, codiva integer, desiva text, disponible numeric, preciobruto numeric, precioneto numeric, costoultimo numeric)
    LANGUAGE plpgsql
    AS $$
BEGIN
    IF UPPER(p_tipo) = 'COMPRA' THEN
        -- Lógica para compras
        RETURN QUERY
        SELECT 
            prd.codproducto,
            ('Cod. Barra: ' || prd.codigobarra || ' Descripción: ' || prd.desproducto)::text AS datoproducto,
            cd.codiva,
            t.desiva::text AS desiva,
            (cd.cantidad - COALESCE(dev.total_devuelto, 0)) AS disponible,
            cd.preciobruto,
            cd.precioneto,
            cd.costoultimo
        FROM purchase.comprasdet cd
        INNER JOIN referential.producto prd ON cd.codproducto = prd.codproducto
        INNER JOIN referential.tipoiva t ON cd.codiva = t.codiva
        LEFT JOIN LATERAL (
            SELECT SUM(ncd.cantidaddev) AS total_devuelto
            FROM shared.notacreditodet ncd
            INNER JOIN shared.notacredito nc ON ncd.codnotacredito = nc.codnotacredito
            WHERE nc.codcompra = cd.codcompra
              AND ncd.codproducto = cd.codproducto
        ) dev ON TRUE
        WHERE cd.codcompra = p_codmovimiento
          AND (cd.cantidad - COALESCE(dev.total_devuelto, 0)) > 0
        ORDER BY prd.codproducto;

    ELSIF UPPER(p_tipo) = 'VENTA' THEN
        -- Lógica para ventas
        RETURN QUERY
        SELECT 
            prd.codproducto,
            ('Cod. Barra: ' || prd.codigobarra || ' Descripción: ' || prd.desproducto)::text AS datoproducto,
            vd.codiva,
            t.desiva::text AS desiva,
            (vd.cantidad - COALESCE(dev.total_devuelto, 0)) AS disponible,
            vd.preciobruto,
            vd.precioneto,
            vd.costoultimo
        FROM sales.ventasdet vd
        INNER JOIN referential.producto prd ON vd.codproducto = prd.codproducto
        INNER JOIN referential.tipoiva t ON vd.codiva = t.codiva
        LEFT JOIN LATERAL (
            SELECT SUM(ncd.cantidaddev) AS total_devuelto
            FROM shared.notacreditodet ncd
            INNER JOIN shared.notacredito nc ON ncd.codnotacredito = nc.codnotacredito
            WHERE nc.codventa = vd.codventa
              AND ncd.codproducto = vd.codproducto
        ) dev ON TRUE
        WHERE vd.codventa = p_codmovimiento
          AND (vd.cantidad - COALESCE(dev.total_devuelto, 0)) > 0
        ORDER BY prd.codproducto;
    ELSE
        RAISE EXCEPTION 'Tipo de movimiento inválido: %', p_tipo;
    END IF;
END;
$$;


ALTER FUNCTION shared.fn_notacredito_list_detalle(p_tipo text, p_codmovimiento integer) OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 221 (class 1259 OID 231377)
-- Name: area; Type: TABLE; Schema: access; Owner: postgres
--

CREATE TABLE access.area (
    codarea integer NOT NULL,
    numarea character varying(100),
    descarea character varying(100)
);


ALTER TABLE access.area OWNER TO postgres;

--
-- TOC entry 222 (class 1259 OID 231380)
-- Name: modulo; Type: TABLE; Schema: access; Owner: postgres
--

CREATE TABLE access.modulo (
    codmodulo integer NOT NULL,
    nummodulo character varying(100),
    descmodulo character varying(100)
);


ALTER TABLE access.modulo OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 231383)
-- Name: permisos; Type: TABLE; Schema: access; Owner: postgres
--

CREATE TABLE access.permisos (
    codusuario integer NOT NULL,
    codmodulo integer NOT NULL,
    i boolean,
    u boolean,
    d boolean,
    s boolean
);


ALTER TABLE access.permisos OWNER TO postgres;

--
-- TOC entry 224 (class 1259 OID 231386)
-- Name: rol; Type: TABLE; Schema: access; Owner: postgres
--

CREATE TABLE access.rol (
    codrol integer NOT NULL,
    numrol character varying(100),
    descrol character varying(100)
);


ALTER TABLE access.rol OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 231389)
-- Name: terminal; Type: TABLE; Schema: access; Owner: postgres
--

CREATE TABLE access.terminal (
    codterminal integer NOT NULL,
    numterminal character varying(3),
    desterminal character varying(100),
    pcasociado character varying(100),
    codsucursal integer
);


ALTER TABLE access.terminal OWNER TO postgres;

--
-- TOC entry 226 (class 1259 OID 231392)
-- Name: usuario; Type: TABLE; Schema: access; Owner: postgres
--

CREATE TABLE access.usuario (
    codusuario integer NOT NULL,
    nomusuario character varying(100),
    passusuario character varying(100),
    correo character varying(100),
    codempleado integer NOT NULL,
    codrol integer NOT NULL,
    activo boolean
);


ALTER TABLE access.usuario OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 231395)
-- Name: compras; Type: TABLE; Schema: purchase; Owner: postgres
--

CREATE TABLE purchase.compras (
    codcompra integer NOT NULL,
    codtipocomprobante integer NOT NULL,
    numcompra character varying(15),
    fechacompra timestamp without time zone,
    codproveedor integer NOT NULL,
    finvalideztimbrado character varying(20),
    nrotimbrado character varying(8),
    codsucursal integer NOT NULL,
    codempleado integer NOT NULL,
    codestmov integer NOT NULL,
    condicionpago smallint,
    codmoneda integer NOT NULL,
    cotizacion numeric(18,5),
    observacion character varying(100),
    asentado boolean,
    impreso boolean,
    totaliva numeric(18,5),
    totalexento numeric(18,5),
    totaldescuento numeric(18,5),
    totalgravada numeric(18,5),
    totalcompra numeric(18,5),
    codordenc integer
);


ALTER TABLE purchase.compras OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 231398)
-- Name: comprasdet; Type: TABLE; Schema: purchase; Owner: postgres
--

CREATE TABLE purchase.comprasdet (
    codcompra integer NOT NULL,
    codproducto integer NOT NULL,
    coddepsuc integer NOT NULL,
    codiva integer NOT NULL,
    cantidad numeric(18,5),
    descuento numeric(18,5),
    preciobruto numeric(18,5),
    precioneto numeric(18,5),
    cotizacion1 numeric(18,5),
    costoultimo numeric(18,5)
);


ALTER TABLE purchase.comprasdet OWNER TO postgres;

--
-- TOC entry 229 (class 1259 OID 231401)
-- Name: facturacompracredito; Type: TABLE; Schema: purchase; Owner: postgres
--

CREATE TABLE purchase.facturacompracredito (
    codcompra integer NOT NULL,
    nrocuota numeric(18,5),
    montocuota numeric(18,5),
    saldopendiente numeric(18,5),
    fechavto date
);


ALTER TABLE purchase.facturacompracredito OWNER TO postgres;

--
-- TOC entry 230 (class 1259 OID 231404)
-- Name: ordencompra; Type: TABLE; Schema: purchase; Owner: postgres
--

CREATE TABLE purchase.ordencompra (
    codordenc integer NOT NULL,
    codpresupuestocompra integer,
    codtipocomprobante integer NOT NULL,
    fechaorden timestamp without time zone,
    numordencompra character varying(13),
    codsucursal integer NOT NULL,
    codempleado integer NOT NULL,
    codestmov integer NOT NULL,
    condicionpago smallint,
    codmoneda integer NOT NULL,
    cotizacion numeric(18,5),
    codproveedor integer NOT NULL,
    observacion character varying(100),
    fechagenerado timestamp without time zone,
    impreso boolean,
    totaliva numeric(18,5),
    totalexento numeric(18,5),
    totaldescuento numeric(18,5),
    totalgravada numeric(18,5),
    totalordencompra numeric(18,5)
);


ALTER TABLE purchase.ordencompra OWNER TO postgres;

--
-- TOC entry 231 (class 1259 OID 231407)
-- Name: ordencompradet; Type: TABLE; Schema: purchase; Owner: postgres
--

CREATE TABLE purchase.ordencompradet (
    codordenc integer NOT NULL,
    codproducto integer NOT NULL,
    codiva integer NOT NULL,
    cantidad numeric(18,5),
    descuento numeric(18,5),
    preciobruto numeric(18,5),
    precioneto numeric(18,5),
    cotizacion1 numeric(18,5)
);


ALTER TABLE purchase.ordencompradet OWNER TO postgres;

--
-- TOC entry 232 (class 1259 OID 231410)
-- Name: pedidocompra; Type: TABLE; Schema: purchase; Owner: postgres
--

CREATE TABLE purchase.pedidocompra (
    codpedcompra integer NOT NULL,
    codtipocomprobante integer NOT NULL,
    numpedcompra character varying,
    fechapedcompra timestamp without time zone,
    codestmov integer NOT NULL,
    codempleado integer NOT NULL,
    codsucursal integer NOT NULL
);


ALTER TABLE purchase.pedidocompra OWNER TO postgres;

--
-- TOC entry 233 (class 1259 OID 231415)
-- Name: pedidocompradet; Type: TABLE; Schema: purchase; Owner: postgres
--

CREATE TABLE purchase.pedidocompradet (
    codpedcompra integer NOT NULL,
    codproducto integer NOT NULL,
    cantidad numeric(18,5),
    costoultimo numeric(18,5)
);


ALTER TABLE purchase.pedidocompradet OWNER TO postgres;

--
-- TOC entry 234 (class 1259 OID 231418)
-- Name: presupuestocompra; Type: TABLE; Schema: purchase; Owner: postgres
--

CREATE TABLE purchase.presupuestocompra (
    codpresupuestocompra integer NOT NULL,
    codtipocomprobante integer NOT NULL,
    fechapresupuesto timestamp without time zone,
    numpresupuestocompra character varying(13),
    codproveedor integer NOT NULL,
    contactoprv character varying(100),
    condiciopago smallint,
    codmoneda integer NOT NULL,
    totaliva numeric(18,5),
    totaldescuento numeric(18,5),
    totalexento numeric(18,5),
    totalgravada numeric(18,5),
    totalpresupuestocompra numeric(18,5),
    cotizacion numeric(18,5),
    codempleado integer NOT NULL,
    observacion character varying(100),
    codpedcompra integer,
    codsucursal integer,
    codestmov integer
);


ALTER TABLE purchase.presupuestocompra OWNER TO postgres;

--
-- TOC entry 235 (class 1259 OID 231421)
-- Name: presupuestocompradet; Type: TABLE; Schema: purchase; Owner: postgres
--

CREATE TABLE purchase.presupuestocompradet (
    codpresupuestocompra integer NOT NULL,
    codproducto integer NOT NULL,
    cantidad numeric(18,5),
    preciobruto numeric(18,5),
    precioneto numeric(18,5),
    costoultimo numeric(18,5),
    codiva integer NOT NULL,
    cotizacion1 numeric(18,5)
);


ALTER TABLE purchase.presupuestocompradet OWNER TO postgres;

--
-- TOC entry 236 (class 1259 OID 231424)
-- Name: caja; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.caja (
    codcaja integer NOT NULL,
    numcaja character varying(3),
    descaja character varying(100),
    codsucursal integer NOT NULL,
    habilitado boolean
);


ALTER TABLE referential.caja OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 231427)
-- Name: cajagestion; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.cajagestion (
    codgestion integer NOT NULL,
    codcaja integer NOT NULL,
    codcobrador integer NOT NULL,
    fechaapertura timestamp without time zone,
    fechacierre timestamp without time zone,
    estado boolean,
    montoapertura numeric(18,5),
    montocierre numeric(18,5),
    codterminal integer NOT NULL
);


ALTER TABLE referential.cajagestion OWNER TO postgres;

--
-- TOC entry 238 (class 1259 OID 231430)
-- Name: ciudad; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.ciudad (
    codciudad integer NOT NULL,
    numciudad character varying(100),
    descciudad character varying(100),
    coddpto integer NOT NULL
);


ALTER TABLE referential.ciudad OWNER TO postgres;

--
-- TOC entry 239 (class 1259 OID 231433)
-- Name: cliente; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.cliente (
    codcliente integer NOT NULL,
    nrodoc character varying(100),
    nombre character varying(100),
    apellido character varying(100),
    activo boolean,
    fechaalta timestamp without time zone,
    fechabaja timestamp without time zone,
    codtipoidnt integer NOT NULL,
    direccion character varying(100),
    nrotelef character varying(100),
    codciudad integer NOT NULL,
    codlista integer NOT NULL,
    clientecredito boolean,
    limitecredito numeric(18,5)
);


ALTER TABLE referential.cliente OWNER TO postgres;

--
-- TOC entry 240 (class 1259 OID 231438)
-- Name: cobrador; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.cobrador (
    codcobrador integer NOT NULL,
    numcobrador character varying(10),
    codempleado integer NOT NULL
);


ALTER TABLE referential.cobrador OWNER TO postgres;

--
-- TOC entry 241 (class 1259 OID 231441)
-- Name: comprobanteterminal; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.comprobanteterminal (
    codterminal integer NOT NULL,
    codtipocomprobante integer NOT NULL,
    inicio numeric(18,0),
    fin numeric(18,0),
    actual numeric(18,0),
    nrotimbrado numeric(8,0),
    iniciovalidez date,
    finvalidez date
);


ALTER TABLE referential.comprobanteterminal OWNER TO postgres;

--
-- TOC entry 242 (class 1259 OID 231444)
-- Name: cotizacion; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.cotizacion (
    codcotizacion integer NOT NULL,
    codmoneda integer NOT NULL,
    monto1 numeric(18,5),
    monto2 numeric(18,5),
    fechacotizacion timestamp without time zone
);


ALTER TABLE referential.cotizacion OWNER TO postgres;

--
-- TOC entry 243 (class 1259 OID 231447)
-- Name: departamento; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.departamento (
    coddpto integer NOT NULL,
    numdpto character varying(100),
    descdpto character varying(100),
    codpais integer NOT NULL
);


ALTER TABLE referential.departamento OWNER TO postgres;

--
-- TOC entry 244 (class 1259 OID 231450)
-- Name: empleado; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.empleado (
    codempleado integer NOT NULL,
    numdoc character varying(100),
    nombre_emp character varying(100),
    apellido_emp character varying(100),
    codarea integer NOT NULL
);


ALTER TABLE referential.empleado OWNER TO postgres;

--
-- TOC entry 245 (class 1259 OID 231453)
-- Name: estadomovimiento; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.estadomovimiento (
    codestmov integer NOT NULL,
    numestmov character varying(100),
    desestmov character varying(100)
);


ALTER TABLE referential.estadomovimiento OWNER TO postgres;

--
-- TOC entry 246 (class 1259 OID 231456)
-- Name: familia; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.familia (
    codfamilia integer NOT NULL,
    numfamilia character varying(3),
    desfamilia character varying(100)
);


ALTER TABLE referential.familia OWNER TO postgres;

--
-- TOC entry 247 (class 1259 OID 231459)
-- Name: formacobro; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.formacobro (
    codformacobro integer NOT NULL,
    numformacobro character varying(100),
    desformacobro character varying(100)
);


ALTER TABLE referential.formacobro OWNER TO postgres;

--
-- TOC entry 248 (class 1259 OID 231462)
-- Name: marca; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.marca (
    codmarca integer NOT NULL,
    nummarca character varying(3),
    desmarca character varying(100),
    soloservicio boolean
);


ALTER TABLE referential.marca OWNER TO postgres;

--
-- TOC entry 249 (class 1259 OID 231465)
-- Name: moneda; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.moneda (
    codmoneda integer NOT NULL,
    nummoneda character varying(3),
    desmoneda character varying(100),
    monedaprincipal boolean
);


ALTER TABLE referential.moneda OWNER TO postgres;

--
-- TOC entry 250 (class 1259 OID 231468)
-- Name: motivoajuste; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.motivoajuste (
    codmotivo integer NOT NULL,
    nummotivo character varying(100),
    desmotivo character varying(100)
);


ALTER TABLE referential.motivoajuste OWNER TO postgres;

--
-- TOC entry 251 (class 1259 OID 231471)
-- Name: movimiento; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.movimiento (
    movimiento character varying(100) NOT NULL
);


ALTER TABLE referential.movimiento OWNER TO postgres;

--
-- TOC entry 252 (class 1259 OID 231474)
-- Name: pais; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.pais (
    codpais integer NOT NULL,
    numpais character varying(100),
    descpaist character varying(100)
);


ALTER TABLE referential.pais OWNER TO postgres;

--
-- TOC entry 253 (class 1259 OID 231477)
-- Name: partesvehiculo; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.partesvehiculo (
    codparte integer NOT NULL,
    numparte character varying(3),
    desparte character varying(100),
    observacion character varying(100)
);


ALTER TABLE referential.partesvehiculo OWNER TO postgres;

--
-- TOC entry 254 (class 1259 OID 231480)
-- Name: precioventaproducto; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.precioventaproducto (
    codproducto integer NOT NULL,
    codlista integer NOT NULL,
    precioventa numeric(18,5),
    codsucursal integer NOT NULL
);


ALTER TABLE referential.precioventaproducto OWNER TO postgres;

--
-- TOC entry 255 (class 1259 OID 231483)
-- Name: procesadoratarjeta; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.procesadoratarjeta (
    codprocedadora integer NOT NULL,
    numprocedadora character varying(100),
    desprocedadora character varying(100)
);


ALTER TABLE referential.procesadoratarjeta OWNER TO postgres;

--
-- TOC entry 256 (class 1259 OID 231486)
-- Name: producto; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.producto (
    codproducto integer NOT NULL,
    codigobarra character varying(100),
    desproducto character varying(100),
    codfamilia integer NOT NULL,
    codmarca integer NOT NULL,
    codrubro integer NOT NULL,
    codunidadmedida integer NOT NULL,
    codiva integer NOT NULL,
    codproveedor integer NOT NULL,
    costoultimo numeric(18,5),
    costopromedio numeric(18,5),
    activo boolean,
    afectastock boolean
);


ALTER TABLE referential.producto OWNER TO postgres;

--
-- TOC entry 257 (class 1259 OID 231489)
-- Name: productosucursal; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.productosucursal (
    codproducto integer NOT NULL,
    codsucursal integer NOT NULL,
    cantidad numeric(18,5),
    cantidad_min numeric(18,5)
);


ALTER TABLE referential.productosucursal OWNER TO postgres;

--
-- TOC entry 258 (class 1259 OID 231492)
-- Name: proveedor; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.proveedor (
    codproveedor integer NOT NULL,
    nrodocprv character varying(100),
    razonsocial character varying(100),
    activo boolean,
    fechaalta timestamp without time zone,
    fechabaja timestamp without time zone,
    codtipoidnt integer NOT NULL,
    direccionprv character varying(100),
    nrotelefprv character varying(100),
    contacto character varying(100),
    codciudad integer NOT NULL,
    nrotimbrado character varying(8),
    fechaventimbrado date
);


ALTER TABLE referential.proveedor OWNER TO postgres;

--
-- TOC entry 259 (class 1259 OID 231497)
-- Name: rubro; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.rubro (
    codrubro integer NOT NULL,
    numrubro character varying(3),
    desrubro character varying(100)
);


ALTER TABLE referential.rubro OWNER TO postgres;

--
-- TOC entry 260 (class 1259 OID 231500)
-- Name: sucursal; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.sucursal (
    codsucursal integer NOT NULL,
    numsucursal character varying(100),
    dessucursal character varying(100),
    direccion character varying(100),
    nrotelefono character varying(100),
    codciudad integer NOT NULL,
    deposito boolean,
    codsucursalpadre integer
);


ALTER TABLE referential.sucursal OWNER TO postgres;

--
-- TOC entry 261 (class 1259 OID 231503)
-- Name: tipo_identificacion; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.tipo_identificacion (
    codtipoidnt integer NOT NULL,
    numtipoidnt character varying(100),
    desctipoidnt character varying(100)
);


ALTER TABLE referential.tipo_identificacion OWNER TO postgres;

--
-- TOC entry 262 (class 1259 OID 231506)
-- Name: tipocomprobante; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.tipocomprobante (
    codtipocomprobante integer NOT NULL,
    numtipocomprobante character varying(100),
    destipocomprobante character varying(100),
    movimiento character varying(100),
    activomov boolean
);


ALTER TABLE referential.tipocomprobante OWNER TO postgres;

--
-- TOC entry 263 (class 1259 OID 231509)
-- Name: tipoiva; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.tipoiva (
    codiva integer NOT NULL,
    numiva character varying(3),
    desiva character varying(100),
    coheficiente numeric(18,5)
);


ALTER TABLE referential.tipoiva OWNER TO postgres;

--
-- TOC entry 264 (class 1259 OID 231512)
-- Name: tipolistaprecio; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.tipolistaprecio (
    codlista integer NOT NULL,
    numlista character varying(3),
    deslista character varying(100)
);


ALTER TABLE referential.tipolistaprecio OWNER TO postgres;

--
-- TOC entry 265 (class 1259 OID 231515)
-- Name: tipotarjeta; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.tipotarjeta (
    codtipotar integer NOT NULL,
    numtipotar character varying(3),
    destipotar character varying(100)
);


ALTER TABLE referential.tipotarjeta OWNER TO postgres;

--
-- TOC entry 266 (class 1259 OID 231518)
-- Name: unidadmedida; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.unidadmedida (
    codunidadmedida integer NOT NULL,
    numunidadmedida character varying(3),
    desunidadmedida character varying(100)
);


ALTER TABLE referential.unidadmedida OWNER TO postgres;

--
-- TOC entry 267 (class 1259 OID 231521)
-- Name: vehiculo; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.vehiculo (
    codvehiculo integer NOT NULL,
    modelo character varying(100),
    nrochapa character varying(100),
    nrochasis character varying(100),
    codcliente integer NOT NULL,
    codmarca integer NOT NULL
);


ALTER TABLE referential.vehiculo OWNER TO postgres;

--
-- TOC entry 268 (class 1259 OID 231524)
-- Name: vendedor; Type: TABLE; Schema: referential; Owner: postgres
--

CREATE TABLE referential.vendedor (
    codvendedor integer NOT NULL,
    numvendedor character varying(10),
    codempleado integer NOT NULL
);


ALTER TABLE referential.vendedor OWNER TO postgres;

--
-- TOC entry 269 (class 1259 OID 231527)
-- Name: facturaventacredito; Type: TABLE; Schema: sales; Owner: postgres
--

CREATE TABLE sales.facturaventacredito (
    codventa integer NOT NULL,
    nrocuota numeric(18,5),
    montocuota numeric(18,5),
    saldopendiente numeric(18,5),
    fechavto date
);


ALTER TABLE sales.facturaventacredito OWNER TO postgres;

--
-- TOC entry 270 (class 1259 OID 231530)
-- Name: pedidoventa; Type: TABLE; Schema: sales; Owner: postgres
--

CREATE TABLE sales.pedidoventa (
    codpedidov integer NOT NULL,
    codtipocomprobante integer NOT NULL,
    codsucursal integer NOT NULL,
    codestmov integer NOT NULL,
    fechapedidov timestamp without time zone,
    numpedventa character varying(15),
    codvendedor integer NOT NULL,
    codcliente integer NOT NULL,
    codmoneda integer NOT NULL,
    totalpedidov numeric(18,5),
    cotizacion1 numeric(18,5)
);


ALTER TABLE sales.pedidoventa OWNER TO postgres;

--
-- TOC entry 271 (class 1259 OID 231533)
-- Name: pedidoventadet; Type: TABLE; Schema: sales; Owner: postgres
--

CREATE TABLE sales.pedidoventadet (
    codpedidov integer NOT NULL,
    codproducto integer NOT NULL,
    cantidad numeric(18,5),
    precioventa numeric(18,5)
);


ALTER TABLE sales.pedidoventadet OWNER TO postgres;

--
-- TOC entry 272 (class 1259 OID 231536)
-- Name: presupuestoventa; Type: TABLE; Schema: sales; Owner: postgres
--

CREATE TABLE sales.presupuestoventa (
    codpresupuestoventa integer NOT NULL,
    codtipocomprobante integer NOT NULL,
    codsucursal integer NOT NULL,
    codvendedor integer NOT NULL,
    codcliente integer NOT NULL,
    fechapresupuestoventa timestamp without time zone,
    numpresupuestoventa character varying(15),
    codpedidov integer,
    observacion character varying(200),
    diaven integer,
    condicionpago smallint,
    codmoneda integer NOT NULL,
    cotizacion1 numeric(18,5),
    codestmov integer NOT NULL,
    totaliva numeric(18,5),
    totaldescuento numeric(18,5),
    totalexento numeric(18,5),
    totalgravada numeric(18,5),
    totalpresupuestoventa numeric(18,5)
);


ALTER TABLE sales.presupuestoventa OWNER TO postgres;

--
-- TOC entry 273 (class 1259 OID 231539)
-- Name: presupuestoventadet; Type: TABLE; Schema: sales; Owner: postgres
--

CREATE TABLE sales.presupuestoventadet (
    codpresupuestoventa integer NOT NULL,
    codproducto integer NOT NULL,
    precioneto numeric(18,5),
    preciobruto numeric(18,5),
    cantidad numeric(18,5),
    costoultimo numeric(18,5),
    codiva integer NOT NULL
);


ALTER TABLE sales.presupuestoventadet OWNER TO postgres;

--
-- TOC entry 274 (class 1259 OID 231542)
-- Name: ventas; Type: TABLE; Schema: sales; Owner: postgres
--

CREATE TABLE sales.ventas (
    codventa integer NOT NULL,
    codtipocomprobante integer NOT NULL,
    numventa character varying(15),
    fechaventa timestamp without time zone,
    codcliente integer NOT NULL,
    finvalideztimbrado character varying(20),
    nrotimbrado character varying(8),
    codsucursal integer NOT NULL,
    codvendedor integer NOT NULL,
    codestmov integer NOT NULL,
    condicionpago smallint,
    codmoneda integer NOT NULL,
    cotizacion numeric(18,5),
    observacion character varying(100),
    asentado boolean,
    impreso boolean,
    totaliva numeric(18,5),
    totalexento numeric(18,5),
    totaldescuento numeric(18,5),
    totalgravada numeric(18,5),
    totalventa numeric(18,5),
    codpresupuestoventa integer
);


ALTER TABLE sales.ventas OWNER TO postgres;

--
-- TOC entry 275 (class 1259 OID 231545)
-- Name: ventasdet; Type: TABLE; Schema: sales; Owner: postgres
--

CREATE TABLE sales.ventasdet (
    codventa integer NOT NULL,
    codproducto integer NOT NULL,
    codiva integer NOT NULL,
    cantidad numeric(18,5),
    descuento numeric(18,5),
    preciobruto numeric(18,5),
    precioneto numeric(18,5),
    cotizacion1 numeric(18,5),
    costoultimo numeric(18,5)
);


ALTER TABLE sales.ventasdet OWNER TO postgres;

--
-- TOC entry 276 (class 1259 OID 231548)
-- Name: diagnosticotecnico; Type: TABLE; Schema: service; Owner: postgres
--

CREATE TABLE service.diagnosticotecnico (
    coddiagnostico integer NOT NULL,
    codtipocomprobante integer NOT NULL,
    codsucursal integer NOT NULL,
    nrodiagnostico character varying(100),
    codestmov integer NOT NULL,
    codempleado integer NOT NULL,
    fechadiagnostico timestamp without time zone,
    codvehiculo integer NOT NULL
);


ALTER TABLE service.diagnosticotecnico OWNER TO postgres;

--
-- TOC entry 277 (class 1259 OID 231551)
-- Name: diagnosticotecnicodet; Type: TABLE; Schema: service; Owner: postgres
--

CREATE TABLE service.diagnosticotecnicodet (
    coddiagnostico integer NOT NULL,
    codparte integer NOT NULL,
    observacion character varying(100)
);


ALTER TABLE service.diagnosticotecnicodet OWNER TO postgres;

--
-- TOC entry 278 (class 1259 OID 231554)
-- Name: registrovehiculo; Type: TABLE; Schema: service; Owner: postgres
--

CREATE TABLE service.registrovehiculo (
    codregistro integer NOT NULL,
    codcliente integer NOT NULL,
    codsucursal integer NOT NULL,
    codempleado integer NOT NULL,
    codtipocomprobante integer NOT NULL,
    numregistro character varying(13),
    fecharegistro timestamp without time zone,
    codvehiculo integer NOT NULL
);


ALTER TABLE service.registrovehiculo OWNER TO postgres;

--
-- TOC entry 279 (class 1259 OID 231557)
-- Name: ajustes; Type: TABLE; Schema: shared; Owner: postgres
--

CREATE TABLE shared.ajustes (
    codajuste integer NOT NULL,
    codtipocomprobante integer NOT NULL,
    codsucursal integer NOT NULL,
    numajuste character varying(100),
    fechaajuste date,
    codmotivo integer NOT NULL,
    codempleado integer NOT NULL,
    condicion smallint
);


ALTER TABLE shared.ajustes OWNER TO postgres;

--
-- TOC entry 280 (class 1259 OID 231560)
-- Name: ajustesdet; Type: TABLE; Schema: shared; Owner: postgres
--

CREATE TABLE shared.ajustesdet (
    codajuste integer NOT NULL,
    codproducto integer NOT NULL,
    cantidad numeric(18,5)
);


ALTER TABLE shared.ajustesdet OWNER TO postgres;

--
-- TOC entry 281 (class 1259 OID 231563)
-- Name: notacredito; Type: TABLE; Schema: shared; Owner: postgres
--

CREATE TABLE shared.notacredito (
    codnotacredito integer NOT NULL,
    codcompra integer,
    codventa integer,
    codproveedor integer,
    codcliente integer,
    codtipocomprobante integer NOT NULL,
    numnotacredito character varying(13),
    nrotimbrado character varying(8),
    fechavalidez date,
    fechanotacredito date,
    codsucursal integer NOT NULL,
    codempleado integer NOT NULL,
    codestmov integer NOT NULL,
    codmoneda integer NOT NULL,
    cotizacion numeric(18,5),
    totaliva numeric(18,5),
    totalexenta numeric(18,5),
    totalgravada numeric(18,5),
    totaldescuento numeric(18,5),
    totaldevolucion numeric(18,5),
    movimiento character varying(100)
);


ALTER TABLE shared.notacredito OWNER TO postgres;

--
-- TOC entry 282 (class 1259 OID 231566)
-- Name: notacreditodet; Type: TABLE; Schema: shared; Owner: postgres
--

CREATE TABLE shared.notacreditodet (
    codnotacredito integer,
    codproducto integer NOT NULL,
    cantidaddev numeric(18,5),
    preciobruto numeric(18,5),
    precioneto numeric(18,5),
    costoultimo numeric(18,5),
    codiva integer NOT NULL
);


ALTER TABLE shared.notacreditodet OWNER TO postgres;

--
-- TOC entry 283 (class 1259 OID 231569)
-- Name: transferencia; Type: TABLE; Schema: shared; Owner: postgres
--

CREATE TABLE shared.transferencia (
    codtransferencia integer NOT NULL,
    codsucursal integer NOT NULL,
    codtipocomprobante integer NOT NULL,
    codestmov integer NOT NULL,
    fechatransferencia date,
    fechaconfirmado date,
    usuariocarga integer NOT NULL,
    usuarioautoriza integer,
    numtransferencia character varying(100),
    observacion character varying(100),
    codsucursaldestino integer NOT NULL,
    costotransferencia numeric(18,5)
);


ALTER TABLE shared.transferencia OWNER TO postgres;

--
-- TOC entry 284 (class 1259 OID 231572)
-- Name: transferenciadet; Type: TABLE; Schema: shared; Owner: postgres
--

CREATE TABLE shared.transferenciadet (
    codtransferencia integer NOT NULL,
    codproducto integer NOT NULL,
    cantidad numeric(18,5),
    costoultimo numeric(18,5)
);


ALTER TABLE shared.transferenciadet OWNER TO postgres;

--
-- TOC entry 5299 (class 0 OID 231377)
-- Dependencies: 221
-- Data for Name: area; Type: TABLE DATA; Schema: access; Owner: postgres
--

COPY access.area (codarea, numarea, descarea) FROM stdin;
1	SOPT	SOPORTE TIC
\.


--
-- TOC entry 5300 (class 0 OID 231380)
-- Dependencies: 222
-- Data for Name: modulo; Type: TABLE DATA; Schema: access; Owner: postgres
--

COPY access.modulo (codmodulo, nummodulo, descmodulo) FROM stdin;
\.


--
-- TOC entry 5301 (class 0 OID 231383)
-- Dependencies: 223
-- Data for Name: permisos; Type: TABLE DATA; Schema: access; Owner: postgres
--

COPY access.permisos (codusuario, codmodulo, i, u, d, s) FROM stdin;
\.


--
-- TOC entry 5302 (class 0 OID 231386)
-- Dependencies: 224
-- Data for Name: rol; Type: TABLE DATA; Schema: access; Owner: postgres
--

COPY access.rol (codrol, numrol, descrol) FROM stdin;
1	ADMIN	ADMIN
\.


--
-- TOC entry 5303 (class 0 OID 231389)
-- Dependencies: 225
-- Data for Name: terminal; Type: TABLE DATA; Schema: access; Owner: postgres
--

COPY access.terminal (codterminal, numterminal, desterminal, pcasociado, codsucursal) FROM stdin;
3	002	NB-CASA	pop-os	1
1	003	PC-LABURO	DESKTOP-JNBH5TS	1
2	001	PC-CASA	DESKTOP-KDVI86I	2
\.


--
-- TOC entry 5304 (class 0 OID 231392)
-- Dependencies: 226
-- Data for Name: usuario; Type: TABLE DATA; Schema: access; Owner: postgres
--

COPY access.usuario (codusuario, nomusuario, passusuario, correo, codempleado, codrol, activo) FROM stdin;
1	enroque	12345	\N	1	1	t
2	enroque2	12345	\N	1	1	t
\.


--
-- TOC entry 5305 (class 0 OID 231395)
-- Dependencies: 227
-- Data for Name: compras; Type: TABLE DATA; Schema: purchase; Owner: postgres
--

COPY purchase.compras (codcompra, codtipocomprobante, numcompra, fechacompra, codproveedor, finvalideztimbrado, nrotimbrado, codsucursal, codempleado, codestmov, condicionpago, codmoneda, cotizacion, observacion, asentado, impreso, totaliva, totalexento, totaldescuento, totalgravada, totalcompra, codordenc) FROM stdin;
4	1	00010001237	2025-08-31 14:22:29.860332	1	2025-12-31	12345678	1	1	4	1	1	1.00000	Compra de prueba con varias sucursales	\N	\N	10000.00000	0.00000	5000.00000	45000.00000	50000.00000	\N
5	1	string	2025-08-31 00:00:00	1	2026-08-31	12345678	1	1	4	0	1	1.00000	string	\N	\N	10.00000	10.00000	10.00000	10.00000	10.00000	\N
15	1	3214213414141	2025-09-22 00:00:00	1	2025-09-25	12421414	1	1	4	1	1	1.00000	adsad	\N	\N	2083.00000	0.00000	0.00000	0.00000	43750.00000	\N
13	1	0010021245789	2025-09-22 00:00:00	1	2025-09-26	12345678	1	1	4	0	1	1.00000	dsadas	\N	\N	786.00000	0.00000	0.00000	0.00000	16500.00000	\N
14	1	5646546546546	2025-09-22 00:00:00	1	2025-09-28	32442354	1	1	4	1	1	1.00000	ddsfsdfdsf	\N	\N	786.00000	0.00000	0.00000	0.00000	16500.00000	\N
16	1	546546546	2025-09-22 00:00:00	1	2025-09-27	23424324	1	1	4	0	1	1.00000	efsdfsdfs	\N	\N	786.00000	0.00000	0.00000	0.00000	16500.00000	\N
17	1	6546546546546	2025-09-22 00:00:00	1	2025-09-28	65465454	1	1	4	0	1	1.00000	sdffsdfsd	\N	\N	1500.00000	0.00000	0.00000	0.00000	16500.00000	\N
18	1	4124124124	2025-09-22 00:00:00	1	2025-09-22	24214214	1	1	4	0	1	1.00000	123412342134	\N	\N	68182.00000	0.00000	0.00000	0.00000	750000.00000	\N
19	1	1321541654654	2025-09-25 00:00:00	1	2025-10-05	65465465	1	1	4	1	1	1.00000	sadadsad	\N	\N	7227.00000	0.00000	0.00000	0.00000	79500.00000	\N
6	2	0010020000010	2025-09-01 00:00:00	1	2025-09-26	12345678	1	1	4	0	1	1.00000	xxxxxx	\N	\N	0.00000	16590.00000	0.00000	0.00000	16590.00000	\N
7	1	00010001267	2025-09-06 13:02:31.738651	1	2025-12-31	12345678	1	1	4	1	1	1.00000	Compra de prueba con varias sucursales	\N	\N	10000.00000	0.00000	5000.00000	45000.00000	50000.00000	\N
1	1	00010001234	2025-08-31 14:16:55.837679	1	2025-12-31	12345678	1	1	4	1	1	1.00000	Compra de prueba con varias sucursales	\N	\N	10000.00000	0.00000	5000.00000	45000.00000	50000.00000	\N
8	1	1234567891234	2025-09-22 00:00:00	1	2025-09-25	12345678	1	1	4	0	1	1.00000	dsadfsad	\N	\N	786.00000	0.00000	0.00000	0.00000	16500.00000	\N
9	1	213213132213	2025-09-22 00:00:00	1	2025-09-25	12232132	1	1	4	0	1	1.00000	ssaddsa	\N	\N	1636.00000	0.00000	0.00000	0.00000	18000.00000	\N
10	1	xxxx	2025-09-22 00:00:00	1	string	string	1	1	4	1	1	1.00000	string	\N	\N	0.00000	0.00000	0.00000	0.00000	165000.00000	\N
11	1	54765465	2025-09-22 00:00:00	1	2025-09-25	1241654	1	1	4	0	1	1.00000	sdadsa	\N	\N	7857.00000	0.00000	0.00000	0.00000	165000.00000	\N
12	1	21321313213	2025-09-22 00:00:00	1	2025-09-23	12232132	1	1	4	1	1	1.00000	sdas	\N	\N	1821.00000	0.00000	0.00000	0.00000	38250.00000	\N
2	1	00010001235	2025-08-31 14:21:05.065414	1	2025-12-31	12345678	1	1	4	1	1	1.00000	Compra de prueba con varias sucursales	\N	\N	10000.00000	0.00000	5000.00000	45000.00000	50000.00000	\N
3	1	00010001236	2025-08-31 14:21:53.665505	1	2025-12-31	12345678	1	1	4	1	1	1.00000	Compra de prueba con varias sucursales	\N	\N	10000.00000	0.00000	5000.00000	45000.00000	50000.00000	\N
\.


--
-- TOC entry 5306 (class 0 OID 231398)
-- Dependencies: 228
-- Data for Name: comprasdet; Type: TABLE DATA; Schema: purchase; Owner: postgres
--

COPY purchase.comprasdet (codcompra, codproducto, coddepsuc, codiva, cantidad, descuento, preciobruto, precioneto, cotizacion1, costoultimo) FROM stdin;
1	1	2	1	10.00000	0.00000	10000.00000	9500.00000	7300.00000	9500.00000
1	2	1	1	5.00000	500.00000	20000.00000	19500.00000	7300.00000	19500.00000
2	1	2	1	10.00000	0.00000	10000.00000	9500.00000	7300.00000	9500.00000
2	2	1	1	5.00000	500.00000	20000.00000	19500.00000	7300.00000	19500.00000
3	1	2	1	10.00000	0.00000	10000.00000	9500.00000	7300.00000	9500.00000
3	2	1	1	5.00000	500.00000	20000.00000	19500.00000	7300.00000	19500.00000
4	1	2	1	10.00000	0.00000	10000.00000	9500.00000	7300.00000	9500.00000
4	2	1	1	5.00000	500.00000	20000.00000	19500.00000	7300.00000	19500.00000
5	1	1	1	10.00000	0.00000	10.00000	10.00000	1.00000	10.00000
6	3	2	3	10.00000	0.00000	1659.00000	1659.00000	0.00000	1500.00000
7	1	2	1	10.00000	0.00000	10000.00000	9500.00000	7300.00000	9500.00000
7	2	1	1	5.00000	500.00000	20000.00000	19500.00000	7300.00000	19500.00000
8	2	1	1	10.00000	0.00000	1650.00000	1571.43000	0.00000	1650.00000
9	1	1	2	10.00000	0.00000	1800.00000	1636.36000	0.00000	1500.00000
10	1	2	1	1.00000	0.00000	0.00000	0.00000	0.00000	0.00000
11	2	1	1	100.00000	0.00000	1650.00000	1571.43000	0.00000	1650.00000
12	2	1	1	15.00000	0.00000	2550.00000	2428.57000	0.00000	1650.00000
13	2	1	1	10.00000	0.00000	1650.00000	1571.43000	0.00000	1650.00000
14	2	1	1	10.00000	0.00000	1650.00000	1571.43000	0.00000	1650.00000
15	2	1	1	35.00000	0.00000	1250.00000	1190.48000	0.00000	1650.00000
16	2	1	1	10.00000	0.00000	1650.00000	1571.43000	0.00000	1650.00000
17	1	1	2	10.00000	0.00000	1650.00000	1500.00000	0.00000	1500.00000
18	1	1	2	30.00000	0.00000	25000.00000	22727.27000	0.00000	1500.00000
19	1	1	2	30.00000	0.00000	2650.00000	2409.09000	0.00000	1500.00000
\.


--
-- TOC entry 5307 (class 0 OID 231401)
-- Dependencies: 229
-- Data for Name: facturacompracredito; Type: TABLE DATA; Schema: purchase; Owner: postgres
--

COPY purchase.facturacompracredito (codcompra, nrocuota, montocuota, saldopendiente, fechavto) FROM stdin;
\.


--
-- TOC entry 5308 (class 0 OID 231404)
-- Dependencies: 230
-- Data for Name: ordencompra; Type: TABLE DATA; Schema: purchase; Owner: postgres
--

COPY purchase.ordencompra (codordenc, codpresupuestocompra, codtipocomprobante, fechaorden, numordencompra, codsucursal, codempleado, codestmov, condicionpago, codmoneda, cotizacion, codproveedor, observacion, fechagenerado, impreso, totaliva, totalexento, totaldescuento, totalgravada, totalordencompra) FROM stdin;
4	\N	4	2025-08-04 00:00:00	0010020000007	1	1	1	0	1	1.00000	1	dsadsad	\N	\N	1191.00000	0.00000	0.00000	0.00000	25000.00000
6	6	1	2025-09-21 00:00:00	0010010000006	1	1	1	0	1	1.00000	1	assdffsd	\N	\N	786.00000	0.00000	0.00000	0.00000	16500.00000
1	\N	4	2025-07-21 11:17:48.31	1	1	1	1	0	1	1.00000	1	\N	\N	\N	0.00000	0.00000	0.00000	0.00000	0.00000
5	\N	2	2025-08-31 00:00:00	0010020000008	1	1	1	0	1	1.00000	1	Llegara en 30 dias	\N	\N	4546.00000	250000.00000	0.00000	0.00000	300000.00000
7	\N	1	2025-09-21 00:00:00	0010010000007	1	1	1	0	1	1.00000	2	sdafdsfsd	\N	\N	1500.00000	0.00000	0.00000	0.00000	16500.00000
2	\N	4	2025-07-21 00:00:00	2	1	1	1	0	1	1.00000	1	string	\N	\N	0.00000	0.00000	0.00000	0.00000	0.00000
3	\N	4	2025-08-04 00:00:00	0010020000006	1	1	2	0	1	1.00000	1		\N	\N	2550.00000	0.00000	0.00000	0.00000	0.00000
\.


--
-- TOC entry 5309 (class 0 OID 231407)
-- Dependencies: 231
-- Data for Name: ordencompradet; Type: TABLE DATA; Schema: purchase; Owner: postgres
--

COPY purchase.ordencompradet (codordenc, codproducto, codiva, cantidad, descuento, preciobruto, precioneto, cotizacion1) FROM stdin;
2	1	1	10.00000	\N	0.00000	0.00000	\N
3	1	2	15.00000	\N	1870.00000	1700.00000	\N
4	2	1	10.00000	\N	2500.00000	2380.95000	\N
5	1	2	10.00000	\N	5000.00000	4545.45000	\N
5	3	3	10.00000	\N	25000.00000	25000.00000	\N
6	2	1	10.00000	\N	1650.00000	1571.43000	\N
7	1	2	10.00000	\N	1650.00000	1500.00000	\N
\.


--
-- TOC entry 5310 (class 0 OID 231410)
-- Dependencies: 232
-- Data for Name: pedidocompra; Type: TABLE DATA; Schema: purchase; Owner: postgres
--

COPY purchase.pedidocompra (codpedcompra, codtipocomprobante, numpedcompra, fechapedcompra, codestmov, codempleado, codsucursal) FROM stdin;
1	1	001-001-0000001	2025-06-21 21:58:58.154	3	1	1
3	1	001-001-0000003	2025-06-23 00:00:00	4	1	1
7	1	001-002-0000009	2025-06-27 00:00:00	1	1	1
8	1	001-002-0000010	2025-06-27 00:00:00	1	1	1
0	1	001-002-0000004	2025-06-23 00:00:00	4	1	1
4	1	001-002-0000005	2025-06-23 00:00:00	4	1	1
11	1	001-002-0000013	2025-07-01 00:00:00	1	1	1
12	1	001-002-0000016	2025-07-03 00:00:00	1	1	1
13	1	001-002-0000020	2025-07-03 00:00:00	1	1	1
14	1	001-002-0000021	2025-07-03 00:00:00	1	1	1
15	1	001-002-0000022	2025-07-03 00:00:00	1	1	1
16	1	001-002-0000023	2025-07-03 00:00:00	1	1	1
9	1	001-002-0000011	2025-06-27 00:00:00	4	1	1
17	1	001-002-0000024	2025-07-04 00:00:00	1	1	1
18	1	001-002-0000001	2025-07-04 00:00:00	1	1	2
19	1	001-002-0000001	2025-07-04 00:00:00	1	1	1
20	1	0010020000004	2025-07-20 00:00:00	1	1	1
22	1	0010010000004	2025-09-19 00:00:00	1	1	2
2	1	001-001-0000002	2025-06-21 22:58:46.872	1	1	1
6	1	001-002-0000008	2025-06-27 00:00:00	4	1	1
5	1	001-002-0000005	2025-06-23 00:00:00	4	1	1
10	1	001-002-0000012	2025-06-30 00:00:00	4	1	1
21	1	0010020000003	2025-09-13 00:00:00	4	1	1
23	1	0010010000024	2025-11-02 00:00:00	1	1	1
\.


--
-- TOC entry 5311 (class 0 OID 231415)
-- Dependencies: 233
-- Data for Name: pedidocompradet; Type: TABLE DATA; Schema: purchase; Owner: postgres
--

COPY purchase.pedidocompradet (codpedcompra, codproducto, cantidad, costoultimo) FROM stdin;
3	1	2.00000	1750.00000
3	2	5.00000	1750.00000
4	1	5.00000	0.00000
5	1	5.00000	0.00000
6	1	10.00000	0.00000
7	1	35.00000	0.00000
8	2	30.00000	1650.00000
9	1	210.00000	1500.00000
10	2	1.00000	1650.00000
11	2	1.00000	1650.00000
12	2	30.00000	1650.00000
3	1	2.00000	1750.00000
3	2	5.00000	1750.00000
4	1	5.00000	0.00000
5	1	5.00000	0.00000
6	1	10.00000	0.00000
7	1	35.00000	0.00000
8	2	30.00000	1650.00000
9	1	210.00000	1500.00000
10	2	1.00000	1650.00000
13	1	10.00000	1650.00000
14	1	10.00000	1650.00000
15	2	20.00000	1650.00000
16	2	20.00000	1650.00000
17	2	30.00000	1650.00000
18	2	30.00000	1650.00000
19	2	2.00000	1650.00000
20	2	10.00000	1650.00000
2	1	2.00000	1500.00000
1	1	10.00000	1500.00000
22	1	10.00000	1500.00000
23	2	10.00000	1650.00000
23	1	10.00000	1500.00000
23	3	10.00000	1500.00000
\.


--
-- TOC entry 5312 (class 0 OID 231418)
-- Dependencies: 234
-- Data for Name: presupuestocompra; Type: TABLE DATA; Schema: purchase; Owner: postgres
--

COPY purchase.presupuestocompra (codpresupuestocompra, codtipocomprobante, fechapresupuesto, numpresupuestocompra, codproveedor, contactoprv, condiciopago, codmoneda, totaliva, totaldescuento, totalexento, totalgravada, totalpresupuestocompra, cotizacion, codempleado, observacion, codpedcompra, codsucursal, codestmov) FROM stdin;
4	3	2025-07-20 00:00:00	0010020000001	1	Juan - 09	0	1	738.00000	0.00000	0.00000	0.00000	15500.00000	1.00000	1	dfdsf	2	1	1
2	3	2025-07-11 00:00:00	0010010000002	1		0	1	500.00000	0.00000	0.00000	5000.00000	5500.00000	1.00000	1	prueba	2	1	1
7	1	2025-07-21 00:00:00	100	1	string	0	1	0.00000	0.00000	0.00000	0.00000	0.00000	0.00000	1	string	2	1	1
5	3	2025-07-20 00:00:00	0010020000004	1	Juan - 09	0	1	738.00000	0.00000	0.00000	0.00000	15500.00000	1.00000	1	sadsad	2	1	1
3	3	2025-07-20 00:00:00	0010020000003	1	Juan - 09	0	1	0.00000	0.00000	0.00000	0.00000	0.00000	1.00000	1		\N	1	1
6	3	2025-07-20 00:00:00	0010020000005	1	Juan - 09	0	1	2619.00000	0.00000	0.00000	0.00000	55000.00000	1.00000	1	dsadsad	2	1	2
8	1	2025-09-21 00:00:00	0010020000005	2	5+4654	1	1	750.00000	0.00000	0.00000	0.00000	8250.00000	1.00000	1	5646546	\N	2	2
\.


--
-- TOC entry 5313 (class 0 OID 231421)
-- Dependencies: 235
-- Data for Name: presupuestocompradet; Type: TABLE DATA; Schema: purchase; Owner: postgres
--

COPY purchase.presupuestocompradet (codpresupuestocompra, codproducto, cantidad, preciobruto, precioneto, costoultimo, codiva, cotizacion1) FROM stdin;
2	1	5.00000	3550.00000	3225.00000	1650.00000	1	\N
3	2	10.00000	1650.00000	1571.43000	1650.00000	1	\N
4	2	10.00000	1550.00000	1476.19000	1650.00000	1	\N
5	2	10.00000	1550.00000	1476.19000	1650.00000	1	\N
6	2	10.00000	5500.00000	5238.10000	1650.00000	1	\N
7	1	1.00000	0.00000	0.00000	0.00000	1	\N
2	2	5.00000	3550.00000	3225.00000	1650.00000	1	\N
8	1	5.00000	1650.00000	1500.00000	1500.00000	2	\N
\.


--
-- TOC entry 5314 (class 0 OID 231424)
-- Dependencies: 236
-- Data for Name: caja; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.caja (codcaja, numcaja, descaja, codsucursal, habilitado) FROM stdin;
1	001	CAJA 1	1	t
2	002	CAJA 2	1	t
3	001	CAJA 1	2	t
4	002	CAJA 2	2	t
\.


--
-- TOC entry 5315 (class 0 OID 231427)
-- Dependencies: 237
-- Data for Name: cajagestion; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.cajagestion (codgestion, codcaja, codcobrador, fechaapertura, fechacierre, estado, montoapertura, montocierre, codterminal) FROM stdin;
3	1	1	2025-10-11 22:41:00	-infinity	t	10000.00000	0.00000	2
4	2	2	2025-10-11 22:41:00	-infinity	t	100000.00000	0.00000	2
9	1	1	2025-10-11 19:54:49.809	-infinity	t	1.00000	0.00000	1
10	2	1	2025-10-11 20:00:54.653	-infinity	t	1.00000	0.00000	1
11	1	1	2025-10-12 13:10:00	2025-10-12 13:10:00	t	100000.00000	100000.00000	2
12	1	1	2025-10-13 19:28:00	2025-10-13 19:28:00	t	10000.00000	100000.00000	2
13	3	1	2025-10-19 21:42:00	2025-10-19 21:43:00	t	411111.00000	10000.00000	2
14	3	1	2025-11-02 03:23:00	2025-11-02 03:45:00	t	10000.00000	100000.00000	2
15	3	1	2025-11-02 17:31:00	2025-11-02 17:31:00	t	1000.00000	100000.00000	2
16	3	1	2025-11-02 18:00:00	\N	f	10000.00000	\N	2
\.


--
-- TOC entry 5316 (class 0 OID 231430)
-- Dependencies: 238
-- Data for Name: ciudad; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.ciudad (codciudad, numciudad, descciudad, coddpto) FROM stdin;
1	LAMB	LAMBARE	1
\.


--
-- TOC entry 5317 (class 0 OID 231433)
-- Dependencies: 239
-- Data for Name: cliente; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.cliente (codcliente, nrodoc, nombre, apellido, activo, fechaalta, fechabaja, codtipoidnt, direccion, nrotelef, codciudad, codlista, clientecredito, limitecredito) FROM stdin;
1	4237259	Enrique	Torales	t	2025-07-25 14:54:04.249	\N	1	\N	\N	1	2	f	0.00000
2	4237260	JuanJose	Shikamaru	t	2025-07-25 14:54:04.249	\N	1	\N	\N	1	2	f	0.00000
\.


--
-- TOC entry 5318 (class 0 OID 231438)
-- Dependencies: 240
-- Data for Name: cobrador; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.cobrador (codcobrador, numcobrador, codempleado) FROM stdin;
1	001	1
2	002	2
\.


--
-- TOC entry 5319 (class 0 OID 231441)
-- Dependencies: 241
-- Data for Name: comprobanteterminal; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.comprobanteterminal (codterminal, codtipocomprobante, inicio, fin, actual, nrotimbrado, iniciovalidez, finvalidez) FROM stdin;
3	1	0	99	3	12345678	2025-07-01	2026-07-30
3	3	0	99	0	12345678	2025-07-01	2026-07-30
3	5	0	99	2	12345678	2025-07-01	2026-07-30
3	6	0	99	3	12345678	2025-07-01	2026-07-30
3	4	0	99	7	12345678	2025-07-01	2026-07-30
3	7	0	99	1	12345678	2025-07-01	2026-07-30
2	3	0	99	98	12345678	2025-07-01	2025-09-13
1	1	0	99	38	12345678	2025-07-01	2026-07-30
1	7	0	99	3	12345678	2025-07-01	2026-07-30
1	9	0	99	0	12345678	2025-10-01	2026-10-30
1	8	0	99	1	12345678	2025-10-01	2026-10-30
1	10	0	99	1	12345678	2025-10-01	2026-10-30
2	5	0	99	3	12345678	2025-07-01	2026-07-30
2	1	0	99	24	12345678	2025-07-01	2026-07-30
2	7	0	99	14	88744478	2025-07-01	2026-07-30
\.


--
-- TOC entry 5320 (class 0 OID 231444)
-- Dependencies: 242
-- Data for Name: cotizacion; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.cotizacion (codcotizacion, codmoneda, monto1, monto2, fechacotizacion) FROM stdin;
\.


--
-- TOC entry 5321 (class 0 OID 231447)
-- Dependencies: 243
-- Data for Name: departamento; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.departamento (coddpto, numdpto, descdpto, codpais) FROM stdin;
1	CEN	CENTRAL	1
\.


--
-- TOC entry 5322 (class 0 OID 231450)
-- Dependencies: 244
-- Data for Name: empleado; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.empleado (codempleado, numdoc, nombre_emp, apellido_emp, codarea) FROM stdin;
1	4237259	ENRIQUE	TORALES	1
2	3421432	fdsf	fdsfdsf	1
\.


--
-- TOC entry 5323 (class 0 OID 231453)
-- Dependencies: 245
-- Data for Name: estadomovimiento; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.estadomovimiento (codestmov, numestmov, desestmov) FROM stdin;
1	PEN	PENDIENTE
2	CON	CONFIRMADO
3	CAN	CANCELADO
4	ANL	ANULADO
5	TRM	TERMINADO
6	ENC	EN CONTROL
8	IMP	NO SE PUEDE REPARAR
9	PAG	PAGADO
7	VER	VERIFICANDO
\.


--
-- TOC entry 5324 (class 0 OID 231456)
-- Dependencies: 246
-- Data for Name: familia; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.familia (codfamilia, numfamilia, desfamilia) FROM stdin;
1	FAM	FAMILIA
\.


--
-- TOC entry 5325 (class 0 OID 231459)
-- Dependencies: 247
-- Data for Name: formacobro; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.formacobro (codformacobro, numformacobro, desformacobro) FROM stdin;
\.


--
-- TOC entry 5326 (class 0 OID 231462)
-- Dependencies: 248
-- Data for Name: marca; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.marca (codmarca, nummarca, desmarca, soloservicio) FROM stdin;
2	SRV	MARCASERV	t
1	MAR	MARCA	f
\.


--
-- TOC entry 5327 (class 0 OID 231465)
-- Dependencies: 249
-- Data for Name: moneda; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.moneda (codmoneda, nummoneda, desmoneda, monedaprincipal) FROM stdin;
1	PYG	GUARANI	t
2	DLS	DOLARES	f
\.


--
-- TOC entry 5328 (class 0 OID 231468)
-- Dependencies: 250
-- Data for Name: motivoajuste; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.motivoajuste (codmotivo, nummotivo, desmotivo) FROM stdin;
1	AJ+	AJUSTE EN POSITIVO
2	AJ-	AJUSTE EN NEGATIVO
4	VEN	VENCIDOS
5	ACT	ACTUALIZACION
6	DAN	DANHADOS
\.


--
-- TOC entry 5329 (class 0 OID 231471)
-- Dependencies: 251
-- Data for Name: movimiento; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.movimiento (movimiento) FROM stdin;
VENTAS
SERVICIOS
COMPRAS
\.


--
-- TOC entry 5330 (class 0 OID 231474)
-- Dependencies: 252
-- Data for Name: pais; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.pais (codpais, numpais, descpaist) FROM stdin;
1	PY	PARAGUAY
\.


--
-- TOC entry 5331 (class 0 OID 231477)
-- Dependencies: 253
-- Data for Name: partesvehiculo; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.partesvehiculo (codparte, numparte, desparte, observacion) FROM stdin;
1	RTD	RETROVISOR DERECHO	
2	RTZ	RETROVISOR IZQUIERDO	
3	PRD	PUERTA DERECHA	
4	PRI	PUERTA IZQUIERDA	
5	CAP	CAPO	
6	BAL	BALIJERA	
7	ESC	ESCAPE	
8	MTR	MOTOR	
9	CJM	CAJA MANUAL	
10	CJT	CAJA AUTOMATICA	
11	CVT	CAJA CVT	
12	ASN	ASIENTOS	\N
\.


--
-- TOC entry 5332 (class 0 OID 231480)
-- Dependencies: 254
-- Data for Name: precioventaproducto; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.precioventaproducto (codproducto, codlista, precioventa, codsucursal) FROM stdin;
1	2	1500.00000	1
2	2	2500.00000	1
3	2	2000.00000	1
1	2	1500.00000	2
3	2	2000.00000	2
\.


--
-- TOC entry 5333 (class 0 OID 231483)
-- Dependencies: 255
-- Data for Name: procesadoratarjeta; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.procesadoratarjeta (codprocedadora, numprocedadora, desprocedadora) FROM stdin;
\.


--
-- TOC entry 5334 (class 0 OID 231486)
-- Dependencies: 256
-- Data for Name: producto; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.producto (codproducto, codigobarra, desproducto, codfamilia, codmarca, codrubro, codunidadmedida, codiva, codproveedor, costoultimo, costopromedio, activo, afectastock) FROM stdin;
2	2222222	PRUEBA2	1	1	1	1	1	1	1650.00000	1900.00000	t	\N
1	1111111	PRUEBA1	1	1	1	1	2	1	1500.00000	1750.00000	t	\N
3	3333333	PRUEBA3	1	1	1	1	3	1	1500.00000	1750.00000	t	\N
\.


--
-- TOC entry 5335 (class 0 OID 231489)
-- Dependencies: 257
-- Data for Name: productosucursal; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.productosucursal (codproducto, codsucursal, cantidad, cantidad_min) FROM stdin;
3	2	10.00000	\N
1	1	20.00000	\N
3	1	30.00000	\N
2	1	64.00000	\N
1	2	11.00000	\N
\.


--
-- TOC entry 5336 (class 0 OID 231492)
-- Dependencies: 258
-- Data for Name: proveedor; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.proveedor (codproveedor, nrodocprv, razonsocial, activo, fechaalta, fechabaja, codtipoidnt, direccionprv, nrotelefprv, contacto, codciudad, nrotimbrado, fechaventimbrado) FROM stdin;
1	001	PRUEBA	t	2025-06-21 22:41:49.022	\N	1	\N	\N	\N	1	12345678	2026-08-01
2	002	PRUEBA2	t	2025-06-21 22:41:49.022	\N	1	\N	\N	\N	1	12345678	2026-08-01
\.


--
-- TOC entry 5337 (class 0 OID 231497)
-- Dependencies: 259
-- Data for Name: rubro; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.rubro (codrubro, numrubro, desrubro) FROM stdin;
1	RBR	RUBRO
\.


--
-- TOC entry 5338 (class 0 OID 231500)
-- Dependencies: 260
-- Data for Name: sucursal; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.sucursal (codsucursal, numsucursal, dessucursal, direccion, nrotelefono, codciudad, deposito, codsucursalpadre) FROM stdin;
1	MTR	MATRIZ	\N	\N	1	\N	\N
2	002	SUC. NRO. 1	\N	\N	1	\N	\N
\.


--
-- TOC entry 5339 (class 0 OID 231503)
-- Dependencies: 261
-- Data for Name: tipo_identificacion; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.tipo_identificacion (codtipoidnt, numtipoidnt, desctipoidnt) FROM stdin;
1	CI	CEDULA IDENTIDAD
2	RUC	RUC
\.


--
-- TOC entry 5340 (class 0 OID 231506)
-- Dependencies: 262
-- Data for Name: tipocomprobante; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.tipocomprobante (codtipocomprobante, numtipocomprobante, destipocomprobante, movimiento, activomov) FROM stdin;
2	FACC	FACTURA COMPRA	COMPRAS	t
1	PEDC	PEDIDO COMPRA	COMPRAS	t
3	PRSTC	PRESUPUESTO COMPRA	COMPRAS	t
4	OC	ORDEN COMPRA	COMPRAS	t
5	REGS	REGISTRO SERVICIO	SERVICIOS	t
6	DGTV	DIAGNOSTICO VEHICULAR	SERVICIOS	t
7	PEDV	PEDIDO VENTA	VENTAS	t
8	FACV	FACTURA VENTA	VENTAS	t
9	PRSTV	PRESUPUESTO VENTA	VENTAS	t
10	NCV	NOTA CREDITO VENTA	VENTAS	t
\.


--
-- TOC entry 5341 (class 0 OID 231509)
-- Dependencies: 263
-- Data for Name: tipoiva; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.tipoiva (codiva, numiva, desiva, coheficiente) FROM stdin;
1	5	IVA 5%	1.05000
2	10	IVA 10%	1.10000
3	0	EXENTO	1.00000
\.


--
-- TOC entry 5342 (class 0 OID 231512)
-- Dependencies: 264
-- Data for Name: tipolistaprecio; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.tipolistaprecio (codlista, numlista, deslista) FROM stdin;
1	MAY	MAYORISTA
2	MIN	MINORISTA
\.


--
-- TOC entry 5343 (class 0 OID 231515)
-- Dependencies: 265
-- Data for Name: tipotarjeta; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.tipotarjeta (codtipotar, numtipotar, destipotar) FROM stdin;
\.


--
-- TOC entry 5344 (class 0 OID 231518)
-- Dependencies: 266
-- Data for Name: unidadmedida; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.unidadmedida (codunidadmedida, numunidadmedida, desunidadmedida) FROM stdin;
1	UN	UNIDAD
\.


--
-- TOC entry 5345 (class 0 OID 231521)
-- Dependencies: 267
-- Data for Name: vehiculo; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.vehiculo (codvehiculo, modelo, nrochapa, nrochasis, codcliente, codmarca) FROM stdin;
1	prueba	prb123	xxxx	1	2
\.


--
-- TOC entry 5346 (class 0 OID 231524)
-- Dependencies: 268
-- Data for Name: vendedor; Type: TABLE DATA; Schema: referential; Owner: postgres
--

COPY referential.vendedor (codvendedor, numvendedor, codempleado) FROM stdin;
1	001	1
\.


--
-- TOC entry 5347 (class 0 OID 231527)
-- Dependencies: 269
-- Data for Name: facturaventacredito; Type: TABLE DATA; Schema: sales; Owner: postgres
--

COPY sales.facturaventacredito (codventa, nrocuota, montocuota, saldopendiente, fechavto) FROM stdin;
2	1.00000	3333.00000	3333.00000	2025-09-26
2	2.00000	3333.00000	3333.00000	2025-10-26
2	3.00000	3333.00000	3333.00000	2025-11-26
3	1.00000	3333.00000	3333.00000	2025-09-26
3	2.00000	3333.00000	3333.00000	2025-10-26
3	3.00000	3333.00000	3333.00000	2025-11-26
4	1.00000	7000.00000	7000.00000	2025-09-26
4	2.00000	7000.00000	7000.00000	2025-10-26
4	3.00000	7000.00000	7000.00000	2025-11-26
4	4.00000	7000.00000	7000.00000	2025-12-26
4	5.00000	7000.00000	7000.00000	2026-01-26
5	1.00000	7000.00000	7000.00000	2025-09-26
5	2.00000	7000.00000	7000.00000	2025-10-26
5	3.00000	7000.00000	7000.00000	2025-11-26
5	4.00000	7000.00000	7000.00000	2025-12-26
5	5.00000	7000.00000	7000.00000	2026-01-26
\.


--
-- TOC entry 5348 (class 0 OID 231530)
-- Dependencies: 270
-- Data for Name: pedidoventa; Type: TABLE DATA; Schema: sales; Owner: postgres
--

COPY sales.pedidoventa (codpedidov, codtipocomprobante, codsucursal, codestmov, fechapedidov, numpedventa, codvendedor, codcliente, codmoneda, totalpedidov, cotizacion1) FROM stdin;
6	1	1	1	2025-09-06 00:00:00	string	1	1	1	0.00000	0.00000
5	7	1	2	2025-09-05 00:00:00	0010020000001	1	2	1	16500.00000	0.00000
7	7	2	1	2025-11-02 00:00:00	0010010000011	1	1	1	16500.00000	1.00000
\.


--
-- TOC entry 5349 (class 0 OID 231533)
-- Dependencies: 271
-- Data for Name: pedidoventadet; Type: TABLE DATA; Schema: sales; Owner: postgres
--

COPY sales.pedidoventadet (codpedidov, codproducto, cantidad, precioventa) FROM stdin;
5	1	10.00000	1650.00000
6	1	1.00000	1600.00000
7	1	10.00000	1650.00000
\.


--
-- TOC entry 5350 (class 0 OID 231536)
-- Dependencies: 272
-- Data for Name: presupuestoventa; Type: TABLE DATA; Schema: sales; Owner: postgres
--

COPY sales.presupuestoventa (codpresupuestoventa, codtipocomprobante, codsucursal, codvendedor, codcliente, fechapresupuestoventa, numpresupuestoventa, codpedidov, observacion, diaven, condicionpago, codmoneda, cotizacion1, codestmov, totaliva, totaldescuento, totalexento, totalgravada, totalpresupuestoventa) FROM stdin;
2	1	1	1	1	2025-09-06 13:09:53.773479	PRST-001	\N	Presupuesto de prueba	30	1	1	1.00000	4	500.00000	50.00000	0.00000	1000.00000	1450.00000
3	1	1	1	1	2025-09-06 00:00:00	0010020000001	\N	prueba	1	0	1	1.00000	4	0.00000	0.00000	0.00000	0.00000	0.00000
4	1	1	1	1	2025-09-06 00:00:00	0010020000002	\N	prueba	1	0	1	1.00000	4	0.00000	0.00000	0.00000	0.00000	0.00000
5	7	1	1	1	2025-09-13 00:00:00	0010020000003	6	sadsa	0	0	1	1.00000	1	750.00000	0.00000	0.00000	0.00000	0.00000
9	7	1	1	1	2025-09-20 00:00:00	0010010000007	\N	xcxxx	7	0	1	1.00000	1	1500.00000	0.00000	0.00000	0.00000	16500.00000
8	7	1	1	2	2025-09-13 00:00:00	0010020000006	\N	dsadd	10	0	1	1.00000	1	625.00000	0.00000	0.00000	0.00000	13125.00000
6	7	1	1	1	2025-09-13 00:00:00	0010020000004	6		5	0	1	1.00000	4	675.00000	0.00000	10000.00000	0.00000	21175.00000
7	7	1	1	1	2025-09-13 00:00:00	0010020000005	\N	dsadsa	10	0	1	5.00000	4	750.00000	0.00000	0.00000	0.00000	8250.00000
1	1	1	1	1	2025-09-06 12:51:00.460691	x	\N	\N	30	1	1	1.00000	1	500.00000	50.00000	0.00000	1000.00000	1450.00000
10	7	2	1	1	2025-11-02 00:00:00	0010010000012	7	1111	30	0	1	1.00000	2	1500.00000	0.00000	0.00000	0.00000	16500.00000
\.


--
-- TOC entry 5351 (class 0 OID 231539)
-- Dependencies: 273
-- Data for Name: presupuestoventadet; Type: TABLE DATA; Schema: sales; Owner: postgres
--

COPY sales.presupuestoventadet (codpresupuestoventa, codproducto, precioneto, preciobruto, cantidad, costoultimo, codiva) FROM stdin;
1	1	1000.00000	1200.00000	2.00000	800.00000	2
1	2	500.00000	600.00000	1.00000	450.00000	1
2	1	1000.00000	1200.00000	2.00000	800.00000	2
2	2	500.00000	600.00000	1.00000	450.00000	1
3	1	0.00000	0.00000	0.00000	0.00000	1
4	1	0.00000	0.00000	0.00000	0.00000	1
5	1	1500.00000	1650.00000	5.00000	0.00000	2
6	1	1500.00000	1650.00000	2.00000	0.00000	2
6	2	2500.00000	2625.00000	3.00000	0.00000	1
6	3	2000.00000	2000.00000	5.00000	0.00000	3
7	1	1500.00000	1650.00000	5.00000	0.00000	2
8	2	2500.00000	2625.00000	5.00000	0.00000	1
9	1	1500.00000	1650.00000	10.00000	0.00000	2
10	1	1500.00000	1650.00000	10.00000	0.00000	2
\.


--
-- TOC entry 5352 (class 0 OID 231542)
-- Dependencies: 274
-- Data for Name: ventas; Type: TABLE DATA; Schema: sales; Owner: postgres
--

COPY sales.ventas (codventa, codtipocomprobante, numventa, fechaventa, codcliente, finvalideztimbrado, nrotimbrado, codsucursal, codvendedor, codestmov, condicionpago, codmoneda, cotizacion, observacion, asentado, impreso, totaliva, totalexento, totaldescuento, totalgravada, totalventa, codpresupuestoventa) FROM stdin;
2	1	00010000123	2025-09-26 10:29:03.685691	1	2025-12-31	12345678	1	1	1	1	1	1.00000	Observación de prueba	\N	\N	1500.00000	0.00000	0.00000	8500.00000	10000.00000	\N
3	1	00010000124	2025-09-26 19:34:28.573581	1	2025-12-31	12345678	1	1	1	1	1	1.00000	Observación de prueba	\N	\N	1500.00000	0.00000	0.00000	8500.00000	10000.00000	\N
4	1	string	2025-05-28 00:00:00	2	string	string	1	1	1	1	1	1.00000	string	\N	\N	0.00000	0.00000	0.00000	0.00000	35000.00000	\N
5	1	strings	2025-05-28 00:00:00	2	string	string	1	1	1	1	1	1.00000	string	\N	\N	0.00000	0.00000	0.00000	0.00000	35000.00000	\N
7	7	0010010000008	2025-09-26 00:00:00	2	2025-09-26	1	1	1	1	0	1	1.00000	sadsadsa	\N	\N	1250.00000	0.00000	0.00000	0.00000	26250.00000	8
8	7	0010010000009	2025-09-26 00:00:00	2	2026-07-30	12345678	1	1	1	0	1	1.00000	sadsadsa	\N	\N	250.00000	0.00000	0.00000	0.00000	5250.00000	8
6	1	stringss	2025-05-28 00:00:00	2	string	string	1	1	2	1	1	1.00000	string	\N	\N	0.00000	0.00000	0.00000	0.00000	35000.00000	\N
9	7	0010010000010	2025-09-26 00:00:00	2	2026-07-30	88744478	1	1	2	0	1	1.00000	sdadsasad	\N	\N	1250.00000	0.00000	0.00000	0.00000	26250.00000	\N
1	1	1	2025-09-26 07:07:03.153	1	1	1	1	1	2	0	1	1.00000	x	f	f	1.00000	1.00000	\N	1.00000	1.00000	\N
10	8	0010030000001	2025-10-01 00:00:00	1	2026-10-30	12345678	1	1	4	0	1	1.00000	DSFDFSD	\N	\N	1250.00000	0.00000	0.00000	0.00000	26250.00000	\N
11	7	0010010000013	2025-11-02 00:00:00	1	2026-07-30	88744478	2	1	1	0	1	1.00000	11111	\N	\N	1500.00000	0.00000	0.00000	0.00000	16500.00000	10
\.


--
-- TOC entry 5353 (class 0 OID 231545)
-- Dependencies: 275
-- Data for Name: ventasdet; Type: TABLE DATA; Schema: sales; Owner: postgres
--

COPY sales.ventasdet (codventa, codproducto, codiva, cantidad, descuento, preciobruto, precioneto, cotizacion1, costoultimo) FROM stdin;
1	1	1	20.00000	0.00000	1650.00000	1500.00000	1.00000	1350.00000
1	2	2	30.00000	0.00000	1650.00000	1500.00000	1.00000	1350.00000
2	1	2	5.00000	0.00000	2000.00000	1900.00000	1.00000	1500.00000
2	2	1	3.00000	0.00000	3000.00000	2800.00000	1.00000	2500.00000
3	1	2	5.00000	0.00000	2000.00000	1900.00000	1.00000	1500.00000
3	2	1	3.00000	0.00000	3000.00000	2800.00000	1.00000	2500.00000
4	2	2	0.00000	0.00000	0.00000	0.00000	0.00000	0.00000
5	2	2	30.00000	0.00000	0.00000	0.00000	0.00000	0.00000
6	2	2	30.00000	0.00000	0.00000	0.00000	0.00000	0.00000
6	1	2	30.00000	0.00000	0.00000	0.00000	0.00000	0.00000
7	2	1	10.00000	0.00000	2625.00000	2500.00000	0.00000	0.00000
8	2	1	2.00000	0.00000	2625.00000	2500.00000	0.00000	0.00000
9	2	1	10.00000	0.00000	2625.00000	2500.00000	0.00000	0.00000
10	2	1	10.00000	0.00000	2625.00000	2500.00000	0.00000	0.00000
11	1	2	10.00000	0.00000	1650.00000	1500.00000	0.00000	0.00000
\.


--
-- TOC entry 5354 (class 0 OID 231548)
-- Dependencies: 276
-- Data for Name: diagnosticotecnico; Type: TABLE DATA; Schema: service; Owner: postgres
--

COPY service.diagnosticotecnico (coddiagnostico, codtipocomprobante, codsucursal, nrodiagnostico, codestmov, codempleado, fechadiagnostico, codvehiculo) FROM stdin;
1	6	1	prb123	3	1	2025-07-26 20:51:41.84	1
3	6	1	0010020000003	1	1	2025-07-27 00:00:00	1
2	1	1	string	4	1	2025-07-27 00:00:00	1
4	5	1	0010010000003	4	1	2025-11-02 00:00:00	1
\.


--
-- TOC entry 5355 (class 0 OID 231551)
-- Dependencies: 277
-- Data for Name: diagnosticotecnicodet; Type: TABLE DATA; Schema: service; Owner: postgres
--

COPY service.diagnosticotecnicodet (coddiagnostico, codparte, observacion) FROM stdin;
1	1	Raspones y una abolladur
2	1	string
3	11	Muerto falta cambiar
4	10	Pierde Aceite
\.


--
-- TOC entry 5356 (class 0 OID 231554)
-- Dependencies: 278
-- Data for Name: registrovehiculo; Type: TABLE DATA; Schema: service; Owner: postgres
--

COPY service.registrovehiculo (codregistro, codcliente, codsucursal, codempleado, codtipocomprobante, numregistro, fecharegistro, codvehiculo) FROM stdin;
1	1	1	1	5	prueba	2025-07-26 00:00:00	1
2	1	1	1	5	prueba2	2025-07-26 00:00:00	1
\.


--
-- TOC entry 5357 (class 0 OID 231557)
-- Dependencies: 279
-- Data for Name: ajustes; Type: TABLE DATA; Schema: shared; Owner: postgres
--

COPY shared.ajustes (codajuste, codtipocomprobante, codsucursal, numajuste, fechaajuste, codmotivo, codempleado, condicion) FROM stdin;
1	1	1	1	2025-09-29	1	1	0
2	1	1	AJ-0001	2025-09-27	2	1	1
3	1	1	string	2025-05-30	1	1	0
4	1	1	0010010000023	2025-09-29	1	1	1
5	1	1	0010030000032	2025-09-30	1	1	1
6	1	1	0010030000033	2025-09-30	1	1	1
7	1	1	0010030000034	2025-09-30	1	1	0
8	1	1	0010030000035	2025-09-30	1	1	1
9	1	1	0010030000036	2025-09-30	1	1	0
10	1	1	0010030000037	2025-09-30	1	1	0
\.


--
-- TOC entry 5358 (class 0 OID 231560)
-- Dependencies: 280
-- Data for Name: ajustesdet; Type: TABLE DATA; Schema: shared; Owner: postgres
--

COPY shared.ajustesdet (codajuste, codproducto, cantidad) FROM stdin;
2	1	5.00000
2	2	3.00000
3	2	30.00000
4	1	30.00000
5	2	30.00000
6	1	20.00000
6	2	15.00000
7	1	30.00000
8	1	100.00000
8	2	50.00000
9	1	20.00000
10	3	30.00000
\.


--
-- TOC entry 5359 (class 0 OID 231563)
-- Dependencies: 281
-- Data for Name: notacredito; Type: TABLE DATA; Schema: shared; Owner: postgres
--

COPY shared.notacredito (codnotacredito, codcompra, codventa, codproveedor, codcliente, codtipocomprobante, numnotacredito, nrotimbrado, fechavalidez, fechanotacredito, codsucursal, codempleado, codestmov, codmoneda, cotizacion, totaliva, totalexenta, totalgravada, totaldescuento, totaldevolucion, movimiento) FROM stdin;
1	1	\N	1	\N	1	NC-00045	123456	2025-12-31	2025-09-27	1	1	1	1	7300.50000	50000.00000	150000.00000	300000.00000	20000.00000	470000.00000	COMPRAS
2	2	\N	1	\N	1	NC-00046	123456	2025-12-31	2025-09-27	1	1	1	1	7300.50000	50000.00000	150000.00000	300000.00000	20000.00000	470000.00000	COMPRAS
3	1	\N	1	\N	1	NC-00047	123456	2025-12-31	2025-09-27	1	1	1	1	7300.50000	50000.00000	150000.00000	300000.00000	20000.00000	470000.00000	COMPRAS
4	18	\N	1	\N	1	5464465465465	46546544	2025-10-03	2025-09-28	1	1	1	1	1.00000	34091.00000	0.00000	0.00000	0.00000	375000.00000	COMPRAS
5	18	\N	1	\N	1	4123454421354	54456465	2025-10-04	2025-09-28	1	1	1	1	1.00000	45455.00000	0.00000	0.00000	0.00000	500000.00000	COMPRAS
6	18	\N	1	\N	1	2321321321321	44454564	2025-10-04	2025-09-28	1	1	1	1	1.00000	22727.00000	0.00000	0.00000	0.00000	250000.00000	COMPRAS
7	3	\N	1	\N	1	2516546546546	45565446	2025-10-05	2025-09-29	1	1	1	1	1.00000	5238.00000	0.00000	0.00000	0.00000	110000.00000	COMPRAS
8	\N	1	\N	1	1	string	string	2025-10-01	2025-10-01	1	1	1	1	0.00000	0.00000	0.00000	0.00000	0.00000	0.00000	VENTAS
9	\N	3	\N	1	7	2312342141234	41342314	2025-10-01	2025-10-01	1	1	1	1	1.00000	286.00000	0.00000	0.00000	0.00000	6000.00000	VENTAS
10	\N	3	\N	1	7	3124123414141	41241414	2025-10-01	2025-10-01	1	1	1	1	1.00000	0.00000	0.00000	0.00000	0.00000	0.00000	VENTAS
11	\N	10	\N	1	10	0010030000001	12345678	2026-10-30	2025-10-01	1	1	1	1	1.00000	625.00000	0.00000	0.00000	0.00000	13125.00000	VENTAS
12	\N	11	\N	1	7	0010010000014	88744478	2026-07-30	2025-11-02	2	1	1	1	1.00000	1500.00000	0.00000	0.00000	0.00000	16500.00000	VENTAS
\.


--
-- TOC entry 5360 (class 0 OID 231566)
-- Dependencies: 282
-- Data for Name: notacreditodet; Type: TABLE DATA; Schema: shared; Owner: postgres
--

COPY shared.notacreditodet (codnotacredito, codproducto, cantidaddev, preciobruto, precioneto, costoultimo, codiva) FROM stdin;
1	1	2.00000	15000.00000	12000.00000	11000.00000	1
1	2	1.00000	30000.00000	25000.00000	24000.00000	1
2	1	2.00000	15000.00000	12000.00000	11000.00000	1
2	2	1.00000	30000.00000	25000.00000	24000.00000	1
3	1	5.00000	15000.00000	12000.00000	11000.00000	1
3	2	2.00000	30000.00000	25000.00000	24000.00000	1
4	1	0.00000	25000.00000	22727.27000	1500.00000	2
5	1	20.00000	25000.00000	22727.27000	1500.00000	2
6	1	10.00000	25000.00000	22727.27000	1500.00000	2
7	2	3.00000	20000.00000	19047.62000	19500.00000	1
7	1	5.00000	10000.00000	9523.81000	9500.00000	1
8	1	0.00000	0.00000	0.00000	0.00000	1
9	2	2.00000	3000.00000	2857.14000	2500.00000	1
11	2	5.00000	2625.00000	2500.00000	0.00000	1
12	1	10.00000	1650.00000	1500.00000	0.00000	2
\.


--
-- TOC entry 5361 (class 0 OID 231569)
-- Dependencies: 283
-- Data for Name: transferencia; Type: TABLE DATA; Schema: shared; Owner: postgres
--

COPY shared.transferencia (codtransferencia, codsucursal, codtipocomprobante, codestmov, fechatransferencia, fechaconfirmado, usuariocarga, usuarioautoriza, numtransferencia, observacion, codsucursaldestino, costotransferencia) FROM stdin;
1	1	1	1	2025-09-29	2025-09-30	1	2	1	\N	2	15000.00000
\.


--
-- TOC entry 5362 (class 0 OID 231572)
-- Dependencies: 284
-- Data for Name: transferenciadet; Type: TABLE DATA; Schema: shared; Owner: postgres
--

COPY shared.transferenciadet (codtransferencia, codproducto, cantidad, costoultimo) FROM stdin;
\.


--
-- TOC entry 4918 (class 2606 OID 231576)
-- Name: area area_pkey; Type: CONSTRAINT; Schema: access; Owner: postgres
--

ALTER TABLE ONLY access.area
    ADD CONSTRAINT area_pkey PRIMARY KEY (codarea);


--
-- TOC entry 4920 (class 2606 OID 231578)
-- Name: modulo modulo_pk; Type: CONSTRAINT; Schema: access; Owner: postgres
--

ALTER TABLE ONLY access.modulo
    ADD CONSTRAINT modulo_pk PRIMARY KEY (codmodulo);


--
-- TOC entry 4922 (class 2606 OID 231580)
-- Name: permisos permisos_pk; Type: CONSTRAINT; Schema: access; Owner: postgres
--

ALTER TABLE ONLY access.permisos
    ADD CONSTRAINT permisos_pk PRIMARY KEY (codusuario, codmodulo);


--
-- TOC entry 4924 (class 2606 OID 231582)
-- Name: rol rol_pk; Type: CONSTRAINT; Schema: access; Owner: postgres
--

ALTER TABLE ONLY access.rol
    ADD CONSTRAINT rol_pk PRIMARY KEY (codrol);


--
-- TOC entry 4926 (class 2606 OID 231584)
-- Name: terminal terminal_pkey; Type: CONSTRAINT; Schema: access; Owner: postgres
--

ALTER TABLE ONLY access.terminal
    ADD CONSTRAINT terminal_pkey PRIMARY KEY (codterminal);


--
-- TOC entry 4928 (class 2606 OID 231586)
-- Name: usuario usuario_pkey; Type: CONSTRAINT; Schema: access; Owner: postgres
--

ALTER TABLE ONLY access.usuario
    ADD CONSTRAINT usuario_pkey PRIMARY KEY (codusuario);


--
-- TOC entry 4930 (class 2606 OID 231588)
-- Name: compras compras_pkey; Type: CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.compras
    ADD CONSTRAINT compras_pkey PRIMARY KEY (codcompra);


--
-- TOC entry 4932 (class 2606 OID 231590)
-- Name: ordencompra ordencompra_pkey; Type: CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.ordencompra
    ADD CONSTRAINT ordencompra_pkey PRIMARY KEY (codordenc);


--
-- TOC entry 4934 (class 2606 OID 231592)
-- Name: pedidocompra pedidocompra_pkey; Type: CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.pedidocompra
    ADD CONSTRAINT pedidocompra_pkey PRIMARY KEY (codpedcompra);


--
-- TOC entry 4936 (class 2606 OID 231594)
-- Name: presupuestocompra presupuestocompra_pkey; Type: CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.presupuestocompra
    ADD CONSTRAINT presupuestocompra_pkey PRIMARY KEY (codpresupuestocompra);


--
-- TOC entry 4938 (class 2606 OID 231596)
-- Name: caja caja_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.caja
    ADD CONSTRAINT caja_pkey PRIMARY KEY (codcaja);


--
-- TOC entry 4940 (class 2606 OID 231598)
-- Name: cajagestion cajagestion_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.cajagestion
    ADD CONSTRAINT cajagestion_pkey PRIMARY KEY (codgestion);


--
-- TOC entry 4942 (class 2606 OID 231600)
-- Name: ciudad ciudad_pk; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.ciudad
    ADD CONSTRAINT ciudad_pk PRIMARY KEY (codciudad);


--
-- TOC entry 4944 (class 2606 OID 231602)
-- Name: cliente cliente_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.cliente
    ADD CONSTRAINT cliente_pkey PRIMARY KEY (codcliente);


--
-- TOC entry 4946 (class 2606 OID 231604)
-- Name: cobrador cobrador_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.cobrador
    ADD CONSTRAINT cobrador_pkey PRIMARY KEY (codcobrador);


--
-- TOC entry 4948 (class 2606 OID 231606)
-- Name: cotizacion cotizacion_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.cotizacion
    ADD CONSTRAINT cotizacion_pkey PRIMARY KEY (codcotizacion);


--
-- TOC entry 4950 (class 2606 OID 231608)
-- Name: departamento departamento_unique; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.departamento
    ADD CONSTRAINT departamento_unique UNIQUE (coddpto);


--
-- TOC entry 4952 (class 2606 OID 231610)
-- Name: empleado empleado_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.empleado
    ADD CONSTRAINT empleado_pkey PRIMARY KEY (codempleado);


--
-- TOC entry 4954 (class 2606 OID 231612)
-- Name: estadomovimiento estadomovimiento_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.estadomovimiento
    ADD CONSTRAINT estadomovimiento_pkey PRIMARY KEY (codestmov);


--
-- TOC entry 4956 (class 2606 OID 231614)
-- Name: familia familia_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.familia
    ADD CONSTRAINT familia_pkey PRIMARY KEY (codfamilia);


--
-- TOC entry 4958 (class 2606 OID 231616)
-- Name: formacobro formacobro_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.formacobro
    ADD CONSTRAINT formacobro_pkey PRIMARY KEY (codformacobro);


--
-- TOC entry 4960 (class 2606 OID 231618)
-- Name: marca marca_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.marca
    ADD CONSTRAINT marca_pkey PRIMARY KEY (codmarca);


--
-- TOC entry 4962 (class 2606 OID 231620)
-- Name: moneda moneda_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.moneda
    ADD CONSTRAINT moneda_pkey PRIMARY KEY (codmoneda);


--
-- TOC entry 4964 (class 2606 OID 231622)
-- Name: motivoajuste motivoajuste_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.motivoajuste
    ADD CONSTRAINT motivoajuste_pkey PRIMARY KEY (codmotivo);


--
-- TOC entry 4966 (class 2606 OID 231624)
-- Name: movimiento movimiento_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.movimiento
    ADD CONSTRAINT movimiento_pkey PRIMARY KEY (movimiento);


--
-- TOC entry 4968 (class 2606 OID 231626)
-- Name: pais pais_unique; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.pais
    ADD CONSTRAINT pais_unique UNIQUE (codpais);


--
-- TOC entry 4970 (class 2606 OID 231628)
-- Name: partesvehiculo partesvehiculo_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.partesvehiculo
    ADD CONSTRAINT partesvehiculo_pkey PRIMARY KEY (codparte);


--
-- TOC entry 4972 (class 2606 OID 231630)
-- Name: procesadoratarjeta procesadoratarjeta_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.procesadoratarjeta
    ADD CONSTRAINT procesadoratarjeta_pkey PRIMARY KEY (codprocedadora);


--
-- TOC entry 4974 (class 2606 OID 231632)
-- Name: producto producto_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.producto
    ADD CONSTRAINT producto_pkey PRIMARY KEY (codproducto);


--
-- TOC entry 4976 (class 2606 OID 231634)
-- Name: productosucursal productosucursal_pk; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.productosucursal
    ADD CONSTRAINT productosucursal_pk PRIMARY KEY (codproducto, codsucursal);


--
-- TOC entry 4978 (class 2606 OID 231636)
-- Name: proveedor proveedor_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.proveedor
    ADD CONSTRAINT proveedor_pkey PRIMARY KEY (codproveedor);


--
-- TOC entry 4980 (class 2606 OID 231638)
-- Name: rubro rubro_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.rubro
    ADD CONSTRAINT rubro_pkey PRIMARY KEY (codrubro);


--
-- TOC entry 4982 (class 2606 OID 231640)
-- Name: sucursal sucursal_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.sucursal
    ADD CONSTRAINT sucursal_pkey PRIMARY KEY (codsucursal);


--
-- TOC entry 4986 (class 2606 OID 231642)
-- Name: tipocomprobante tipocomprobante_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.tipocomprobante
    ADD CONSTRAINT tipocomprobante_pkey PRIMARY KEY (codtipocomprobante);


--
-- TOC entry 4984 (class 2606 OID 231644)
-- Name: tipo_identificacion tipoidnt_pk; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.tipo_identificacion
    ADD CONSTRAINT tipoidnt_pk PRIMARY KEY (codtipoidnt);


--
-- TOC entry 4988 (class 2606 OID 231646)
-- Name: tipoiva tipoiva_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.tipoiva
    ADD CONSTRAINT tipoiva_pkey PRIMARY KEY (codiva);


--
-- TOC entry 4990 (class 2606 OID 231648)
-- Name: tipolistaprecio tipolistaprecio_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.tipolistaprecio
    ADD CONSTRAINT tipolistaprecio_pkey PRIMARY KEY (codlista);


--
-- TOC entry 4992 (class 2606 OID 231650)
-- Name: tipotarjeta tipotarjeta_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.tipotarjeta
    ADD CONSTRAINT tipotarjeta_pkey PRIMARY KEY (codtipotar);


--
-- TOC entry 4994 (class 2606 OID 231652)
-- Name: unidadmedida unidadmedida_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.unidadmedida
    ADD CONSTRAINT unidadmedida_pkey PRIMARY KEY (codunidadmedida);


--
-- TOC entry 4996 (class 2606 OID 231654)
-- Name: vehiculo vehiculo_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.vehiculo
    ADD CONSTRAINT vehiculo_pkey PRIMARY KEY (codvehiculo);


--
-- TOC entry 4998 (class 2606 OID 231656)
-- Name: vendedor vendedor_pkey; Type: CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.vendedor
    ADD CONSTRAINT vendedor_pkey PRIMARY KEY (codvendedor);


--
-- TOC entry 5000 (class 2606 OID 231658)
-- Name: pedidoventa pedidoventa_pkey; Type: CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.pedidoventa
    ADD CONSTRAINT pedidoventa_pkey PRIMARY KEY (codpedidov);


--
-- TOC entry 5002 (class 2606 OID 231660)
-- Name: presupuestoventa presupuestoventa_pkey; Type: CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.presupuestoventa
    ADD CONSTRAINT presupuestoventa_pkey PRIMARY KEY (codpresupuestoventa);


--
-- TOC entry 5004 (class 2606 OID 231662)
-- Name: ventas ventas_pkey; Type: CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.ventas
    ADD CONSTRAINT ventas_pkey PRIMARY KEY (codventa);


--
-- TOC entry 5006 (class 2606 OID 231664)
-- Name: diagnosticotecnico diagnosticotecnico_pkey; Type: CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.diagnosticotecnico
    ADD CONSTRAINT diagnosticotecnico_pkey PRIMARY KEY (coddiagnostico);


--
-- TOC entry 5008 (class 2606 OID 231666)
-- Name: registrovehiculo registrovehiculo_pkey; Type: CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.registrovehiculo
    ADD CONSTRAINT registrovehiculo_pkey PRIMARY KEY (codregistro);


--
-- TOC entry 5010 (class 2606 OID 231668)
-- Name: ajustes ajustes_pkey; Type: CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.ajustes
    ADD CONSTRAINT ajustes_pkey PRIMARY KEY (codajuste);


--
-- TOC entry 5012 (class 2606 OID 231670)
-- Name: notacredito notacredito_pkey; Type: CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacredito
    ADD CONSTRAINT notacredito_pkey PRIMARY KEY (codnotacredito);


--
-- TOC entry 5014 (class 2606 OID 231672)
-- Name: transferencia transferencia_pkey; Type: CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.transferencia
    ADD CONSTRAINT transferencia_pkey PRIMARY KEY (codtransferencia);


--
-- TOC entry 5017 (class 2606 OID 231673)
-- Name: terminal sucursalterminal_fk; Type: FK CONSTRAINT; Schema: access; Owner: postgres
--

ALTER TABLE ONLY access.terminal
    ADD CONSTRAINT sucursalterminal_fk FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5018 (class 2606 OID 231678)
-- Name: usuario usu_emp; Type: FK CONSTRAINT; Schema: access; Owner: postgres
--

ALTER TABLE ONLY access.usuario
    ADD CONSTRAINT usu_emp FOREIGN KEY (codempleado) REFERENCES referential.empleado(codempleado);


--
-- TOC entry 5015 (class 2606 OID 231683)
-- Name: permisos usu_modulo; Type: FK CONSTRAINT; Schema: access; Owner: postgres
--

ALTER TABLE ONLY access.permisos
    ADD CONSTRAINT usu_modulo FOREIGN KEY (codmodulo) REFERENCES access.modulo(codmodulo);


--
-- TOC entry 5016 (class 2606 OID 231688)
-- Name: permisos usu_perm; Type: FK CONSTRAINT; Schema: access; Owner: postgres
--

ALTER TABLE ONLY access.permisos
    ADD CONSTRAINT usu_perm FOREIGN KEY (codusuario) REFERENCES access.usuario(codusuario);


--
-- TOC entry 5019 (class 2606 OID 231693)
-- Name: usuario usu_rol; Type: FK CONSTRAINT; Schema: access; Owner: postgres
--

ALTER TABLE ONLY access.usuario
    ADD CONSTRAINT usu_rol FOREIGN KEY (codrol) REFERENCES access.rol(codrol);


--
-- TOC entry 5020 (class 2606 OID 231698)
-- Name: compras compras_codempleado_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.compras
    ADD CONSTRAINT compras_codempleado_fkey FOREIGN KEY (codempleado) REFERENCES referential.empleado(codempleado);


--
-- TOC entry 5021 (class 2606 OID 231703)
-- Name: compras compras_codestmov_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.compras
    ADD CONSTRAINT compras_codestmov_fkey FOREIGN KEY (codestmov) REFERENCES referential.estadomovimiento(codestmov);


--
-- TOC entry 5022 (class 2606 OID 231708)
-- Name: compras compras_codmoneda_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.compras
    ADD CONSTRAINT compras_codmoneda_fkey FOREIGN KEY (codmoneda) REFERENCES referential.moneda(codmoneda);


--
-- TOC entry 5023 (class 2606 OID 231713)
-- Name: compras compras_codordenc_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.compras
    ADD CONSTRAINT compras_codordenc_fkey FOREIGN KEY (codordenc) REFERENCES purchase.ordencompra(codordenc);


--
-- TOC entry 5024 (class 2606 OID 231718)
-- Name: compras compras_codproveedor_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.compras
    ADD CONSTRAINT compras_codproveedor_fkey FOREIGN KEY (codproveedor) REFERENCES referential.proveedor(codproveedor);


--
-- TOC entry 5025 (class 2606 OID 231723)
-- Name: compras compras_codsucursal_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.compras
    ADD CONSTRAINT compras_codsucursal_fkey FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5026 (class 2606 OID 231728)
-- Name: compras compras_codtipocomprobante_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.compras
    ADD CONSTRAINT compras_codtipocomprobante_fkey FOREIGN KEY (codtipocomprobante) REFERENCES referential.tipocomprobante(codtipocomprobante);


--
-- TOC entry 5027 (class 2606 OID 231733)
-- Name: comprasdet comprasdet_codcompra_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.comprasdet
    ADD CONSTRAINT comprasdet_codcompra_fkey FOREIGN KEY (codcompra) REFERENCES purchase.compras(codcompra);


--
-- TOC entry 5028 (class 2606 OID 231738)
-- Name: comprasdet comprasdet_codiva_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.comprasdet
    ADD CONSTRAINT comprasdet_codiva_fkey FOREIGN KEY (codiva) REFERENCES referential.tipoiva(codiva);


--
-- TOC entry 5029 (class 2606 OID 231743)
-- Name: comprasdet comprasdet_codproducto_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.comprasdet
    ADD CONSTRAINT comprasdet_codproducto_fkey FOREIGN KEY (codproducto) REFERENCES referential.producto(codproducto);


--
-- TOC entry 5030 (class 2606 OID 231748)
-- Name: facturacompracredito facturacompracredito_codcompra_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.facturacompracredito
    ADD CONSTRAINT facturacompracredito_codcompra_fkey FOREIGN KEY (codcompra) REFERENCES purchase.compras(codcompra);


--
-- TOC entry 5031 (class 2606 OID 231753)
-- Name: ordencompra ordencompra_codempleado_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.ordencompra
    ADD CONSTRAINT ordencompra_codempleado_fkey FOREIGN KEY (codempleado) REFERENCES referential.empleado(codempleado);


--
-- TOC entry 5032 (class 2606 OID 231758)
-- Name: ordencompra ordencompra_codestmov_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.ordencompra
    ADD CONSTRAINT ordencompra_codestmov_fkey FOREIGN KEY (codestmov) REFERENCES referential.estadomovimiento(codestmov);


--
-- TOC entry 5033 (class 2606 OID 231763)
-- Name: ordencompra ordencompra_codmoneda_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.ordencompra
    ADD CONSTRAINT ordencompra_codmoneda_fkey FOREIGN KEY (codmoneda) REFERENCES referential.moneda(codmoneda);


--
-- TOC entry 5034 (class 2606 OID 231768)
-- Name: ordencompra ordencompra_codpresupuestocompra_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.ordencompra
    ADD CONSTRAINT ordencompra_codpresupuestocompra_fkey FOREIGN KEY (codpresupuestocompra) REFERENCES purchase.presupuestocompra(codpresupuestocompra);


--
-- TOC entry 5035 (class 2606 OID 231773)
-- Name: ordencompra ordencompra_codproveedor_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.ordencompra
    ADD CONSTRAINT ordencompra_codproveedor_fkey FOREIGN KEY (codproveedor) REFERENCES referential.proveedor(codproveedor);


--
-- TOC entry 5036 (class 2606 OID 231778)
-- Name: ordencompra ordencompra_codsucursal_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.ordencompra
    ADD CONSTRAINT ordencompra_codsucursal_fkey FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5037 (class 2606 OID 231783)
-- Name: ordencompra ordencompra_codtipocomprobante_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.ordencompra
    ADD CONSTRAINT ordencompra_codtipocomprobante_fkey FOREIGN KEY (codtipocomprobante) REFERENCES referential.tipocomprobante(codtipocomprobante);


--
-- TOC entry 5038 (class 2606 OID 231788)
-- Name: ordencompradet ordencompradet_codiva_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.ordencompradet
    ADD CONSTRAINT ordencompradet_codiva_fkey FOREIGN KEY (codiva) REFERENCES referential.tipoiva(codiva);


--
-- TOC entry 5039 (class 2606 OID 231793)
-- Name: ordencompradet ordencompradet_codordenc_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.ordencompradet
    ADD CONSTRAINT ordencompradet_codordenc_fkey FOREIGN KEY (codordenc) REFERENCES purchase.ordencompra(codordenc);


--
-- TOC entry 5040 (class 2606 OID 231798)
-- Name: ordencompradet ordencompradet_codproducto_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.ordencompradet
    ADD CONSTRAINT ordencompradet_codproducto_fkey FOREIGN KEY (codproducto) REFERENCES referential.producto(codproducto);


--
-- TOC entry 5041 (class 2606 OID 231803)
-- Name: pedidocompra pedidocompra_codempleado_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.pedidocompra
    ADD CONSTRAINT pedidocompra_codempleado_fkey FOREIGN KEY (codempleado) REFERENCES referential.empleado(codempleado);


--
-- TOC entry 5042 (class 2606 OID 231808)
-- Name: pedidocompra pedidocompra_codestmov_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.pedidocompra
    ADD CONSTRAINT pedidocompra_codestmov_fkey FOREIGN KEY (codestmov) REFERENCES referential.estadomovimiento(codestmov);


--
-- TOC entry 5043 (class 2606 OID 231813)
-- Name: pedidocompra pedidocompra_codsucursal_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.pedidocompra
    ADD CONSTRAINT pedidocompra_codsucursal_fkey FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5044 (class 2606 OID 231818)
-- Name: pedidocompra pedidocompra_codtipocomprobante_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.pedidocompra
    ADD CONSTRAINT pedidocompra_codtipocomprobante_fkey FOREIGN KEY (codtipocomprobante) REFERENCES referential.tipocomprobante(codtipocomprobante);


--
-- TOC entry 5045 (class 2606 OID 231823)
-- Name: pedidocompradet pedidocompradet_codpedcompra_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.pedidocompradet
    ADD CONSTRAINT pedidocompradet_codpedcompra_fkey FOREIGN KEY (codpedcompra) REFERENCES purchase.pedidocompra(codpedcompra);


--
-- TOC entry 5046 (class 2606 OID 231828)
-- Name: pedidocompradet pedidocompradet_codproducto_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.pedidocompradet
    ADD CONSTRAINT pedidocompradet_codproducto_fkey FOREIGN KEY (codproducto) REFERENCES referential.producto(codproducto);


--
-- TOC entry 5047 (class 2606 OID 231833)
-- Name: presupuestocompra prescompra_estadomov_fk; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.presupuestocompra
    ADD CONSTRAINT prescompra_estadomov_fk FOREIGN KEY (codestmov) REFERENCES referential.estadomovimiento(codestmov);


--
-- TOC entry 5048 (class 2606 OID 231838)
-- Name: presupuestocompra prescompra_sucursal_fk; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.presupuestocompra
    ADD CONSTRAINT prescompra_sucursal_fk FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5054 (class 2606 OID 231843)
-- Name: presupuestocompradet presupuescompradet_codiva_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.presupuestocompradet
    ADD CONSTRAINT presupuescompradet_codiva_fkey FOREIGN KEY (codiva) REFERENCES referential.tipoiva(codiva);


--
-- TOC entry 5055 (class 2606 OID 231848)
-- Name: presupuestocompradet presupuescompradet_codpresupuestocompra_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.presupuestocompradet
    ADD CONSTRAINT presupuescompradet_codpresupuestocompra_fkey FOREIGN KEY (codpresupuestocompra) REFERENCES purchase.presupuestocompra(codpresupuestocompra);


--
-- TOC entry 5056 (class 2606 OID 231853)
-- Name: presupuestocompradet presupuescompradet_codproducto_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.presupuestocompradet
    ADD CONSTRAINT presupuescompradet_codproducto_fkey FOREIGN KEY (codproducto) REFERENCES referential.producto(codproducto);


--
-- TOC entry 5049 (class 2606 OID 231858)
-- Name: presupuestocompra presupuestocompra_codempleado_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.presupuestocompra
    ADD CONSTRAINT presupuestocompra_codempleado_fkey FOREIGN KEY (codempleado) REFERENCES referential.empleado(codempleado);


--
-- TOC entry 5050 (class 2606 OID 231863)
-- Name: presupuestocompra presupuestocompra_codmoneda_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.presupuestocompra
    ADD CONSTRAINT presupuestocompra_codmoneda_fkey FOREIGN KEY (codmoneda) REFERENCES referential.moneda(codmoneda);


--
-- TOC entry 5051 (class 2606 OID 231868)
-- Name: presupuestocompra presupuestocompra_codpedcompra_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.presupuestocompra
    ADD CONSTRAINT presupuestocompra_codpedcompra_fkey FOREIGN KEY (codpedcompra) REFERENCES purchase.pedidocompra(codpedcompra);


--
-- TOC entry 5052 (class 2606 OID 231873)
-- Name: presupuestocompra presupuestocompra_codproveedor_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.presupuestocompra
    ADD CONSTRAINT presupuestocompra_codproveedor_fkey FOREIGN KEY (codproveedor) REFERENCES referential.proveedor(codproveedor);


--
-- TOC entry 5053 (class 2606 OID 231878)
-- Name: presupuestocompra presupuestocompra_codtipocomprobante_fkey; Type: FK CONSTRAINT; Schema: purchase; Owner: postgres
--

ALTER TABLE ONLY purchase.presupuestocompra
    ADD CONSTRAINT presupuestocompra_codtipocomprobante_fkey FOREIGN KEY (codtipocomprobante) REFERENCES referential.tipocomprobante(codtipocomprobante);


--
-- TOC entry 5057 (class 2606 OID 231883)
-- Name: caja caja_codsucursal_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.caja
    ADD CONSTRAINT caja_codsucursal_fkey FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5058 (class 2606 OID 231888)
-- Name: cajagestion cajagestion_codcaja_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.cajagestion
    ADD CONSTRAINT cajagestion_codcaja_fkey FOREIGN KEY (codcaja) REFERENCES referential.caja(codcaja);


--
-- TOC entry 5059 (class 2606 OID 231893)
-- Name: cajagestion cajagestion_codcobrador_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.cajagestion
    ADD CONSTRAINT cajagestion_codcobrador_fkey FOREIGN KEY (codcobrador) REFERENCES referential.cobrador(codcobrador);


--
-- TOC entry 5060 (class 2606 OID 231898)
-- Name: ciudad ciudad_dpto; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.ciudad
    ADD CONSTRAINT ciudad_dpto FOREIGN KEY (coddpto) REFERENCES referential.departamento(coddpto);


--
-- TOC entry 5061 (class 2606 OID 231903)
-- Name: cliente cliente_codciudad_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.cliente
    ADD CONSTRAINT cliente_codciudad_fkey FOREIGN KEY (codciudad) REFERENCES referential.ciudad(codciudad);


--
-- TOC entry 5062 (class 2606 OID 231908)
-- Name: cliente cliente_codlista_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.cliente
    ADD CONSTRAINT cliente_codlista_fkey FOREIGN KEY (codlista) REFERENCES referential.tipolistaprecio(codlista);


--
-- TOC entry 5063 (class 2606 OID 231913)
-- Name: cliente cliente_codtipoidnt_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.cliente
    ADD CONSTRAINT cliente_codtipoidnt_fkey FOREIGN KEY (codtipoidnt) REFERENCES referential.tipo_identificacion(codtipoidnt);


--
-- TOC entry 5064 (class 2606 OID 231923)
-- Name: cobrador cobrador_codempleado_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.cobrador
    ADD CONSTRAINT cobrador_codempleado_fkey FOREIGN KEY (codempleado) REFERENCES referential.empleado(codempleado);


--
-- TOC entry 5065 (class 2606 OID 231928)
-- Name: comprobanteterminal comprobanteterminal_codterminal_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.comprobanteterminal
    ADD CONSTRAINT comprobanteterminal_codterminal_fkey FOREIGN KEY (codterminal) REFERENCES access.terminal(codterminal);


--
-- TOC entry 5066 (class 2606 OID 231933)
-- Name: comprobanteterminal comprobanteterminal_codtipocomprobante_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.comprobanteterminal
    ADD CONSTRAINT comprobanteterminal_codtipocomprobante_fkey FOREIGN KEY (codtipocomprobante) REFERENCES referential.tipocomprobante(codtipocomprobante);


--
-- TOC entry 5067 (class 2606 OID 231938)
-- Name: cotizacion cotizacion_codmoneda_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.cotizacion
    ADD CONSTRAINT cotizacion_codmoneda_fkey FOREIGN KEY (codmoneda) REFERENCES referential.moneda(codmoneda);


--
-- TOC entry 5068 (class 2606 OID 231943)
-- Name: departamento dpto_pais; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.departamento
    ADD CONSTRAINT dpto_pais FOREIGN KEY (codpais) REFERENCES referential.pais(codpais);


--
-- TOC entry 5069 (class 2606 OID 231948)
-- Name: empleado emp_area; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.empleado
    ADD CONSTRAINT emp_area FOREIGN KEY (codarea) REFERENCES access.area(codarea);


--
-- TOC entry 5085 (class 2606 OID 231953)
-- Name: vehiculo fk_marcavehiculo; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.vehiculo
    ADD CONSTRAINT fk_marcavehiculo FOREIGN KEY (codmarca) REFERENCES referential.marca(codmarca);


--
-- TOC entry 5070 (class 2606 OID 231958)
-- Name: precioventaproducto precioventaproducto_codlista_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.precioventaproducto
    ADD CONSTRAINT precioventaproducto_codlista_fkey FOREIGN KEY (codlista) REFERENCES referential.tipolistaprecio(codlista);


--
-- TOC entry 5071 (class 2606 OID 231963)
-- Name: precioventaproducto precioventaproducto_codproducto_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.precioventaproducto
    ADD CONSTRAINT precioventaproducto_codproducto_fkey FOREIGN KEY (codproducto) REFERENCES referential.producto(codproducto);


--
-- TOC entry 5072 (class 2606 OID 231968)
-- Name: precioventaproducto precioventaproducto_codsucursal_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.precioventaproducto
    ADD CONSTRAINT precioventaproducto_codsucursal_fkey FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5073 (class 2606 OID 231973)
-- Name: producto producto_codfamilia_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.producto
    ADD CONSTRAINT producto_codfamilia_fkey FOREIGN KEY (codfamilia) REFERENCES referential.familia(codfamilia);


--
-- TOC entry 5074 (class 2606 OID 231978)
-- Name: producto producto_codiva_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.producto
    ADD CONSTRAINT producto_codiva_fkey FOREIGN KEY (codiva) REFERENCES referential.tipoiva(codiva);


--
-- TOC entry 5075 (class 2606 OID 231983)
-- Name: producto producto_codmarca_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.producto
    ADD CONSTRAINT producto_codmarca_fkey FOREIGN KEY (codmarca) REFERENCES referential.marca(codmarca);


--
-- TOC entry 5076 (class 2606 OID 231988)
-- Name: producto producto_codproveedor_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.producto
    ADD CONSTRAINT producto_codproveedor_fkey FOREIGN KEY (codproveedor) REFERENCES referential.proveedor(codproveedor);


--
-- TOC entry 5077 (class 2606 OID 231993)
-- Name: producto producto_codrubro_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.producto
    ADD CONSTRAINT producto_codrubro_fkey FOREIGN KEY (codrubro) REFERENCES referential.rubro(codrubro);


--
-- TOC entry 5078 (class 2606 OID 231998)
-- Name: producto producto_codunidadmedida_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.producto
    ADD CONSTRAINT producto_codunidadmedida_fkey FOREIGN KEY (codunidadmedida) REFERENCES referential.unidadmedida(codunidadmedida);


--
-- TOC entry 5079 (class 2606 OID 232003)
-- Name: productosucursal productosucursal_codproducto_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.productosucursal
    ADD CONSTRAINT productosucursal_codproducto_fkey FOREIGN KEY (codproducto) REFERENCES referential.producto(codproducto);


--
-- TOC entry 5080 (class 2606 OID 232008)
-- Name: productosucursal productosucursal_codsucursal_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.productosucursal
    ADD CONSTRAINT productosucursal_codsucursal_fkey FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5081 (class 2606 OID 232013)
-- Name: proveedor proveedor_codciudad_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.proveedor
    ADD CONSTRAINT proveedor_codciudad_fkey FOREIGN KEY (codciudad) REFERENCES referential.ciudad(codciudad);


--
-- TOC entry 5082 (class 2606 OID 232018)
-- Name: proveedor proveedor_codtipoidnt_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.proveedor
    ADD CONSTRAINT proveedor_codtipoidnt_fkey FOREIGN KEY (codtipoidnt) REFERENCES referential.tipo_identificacion(codtipoidnt);


--
-- TOC entry 5083 (class 2606 OID 232023)
-- Name: sucursal sucursal_codciudad_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.sucursal
    ADD CONSTRAINT sucursal_codciudad_fkey FOREIGN KEY (codciudad) REFERENCES referential.ciudad(codciudad);


--
-- TOC entry 5084 (class 2606 OID 232028)
-- Name: tipocomprobante tipomov_fk; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.tipocomprobante
    ADD CONSTRAINT tipomov_fk FOREIGN KEY (movimiento) REFERENCES referential.movimiento(movimiento);


--
-- TOC entry 5086 (class 2606 OID 232033)
-- Name: vehiculo vehiculo_codcliente_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.vehiculo
    ADD CONSTRAINT vehiculo_codcliente_fkey FOREIGN KEY (codcliente) REFERENCES referential.cliente(codcliente);


--
-- TOC entry 5087 (class 2606 OID 232038)
-- Name: vendedor vendedor_codempleado_fkey; Type: FK CONSTRAINT; Schema: referential; Owner: postgres
--

ALTER TABLE ONLY referential.vendedor
    ADD CONSTRAINT vendedor_codempleado_fkey FOREIGN KEY (codempleado) REFERENCES referential.empleado(codempleado);


--
-- TOC entry 5088 (class 2606 OID 232043)
-- Name: facturaventacredito facturaventacredito_codventa_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.facturaventacredito
    ADD CONSTRAINT facturaventacredito_codventa_fkey FOREIGN KEY (codventa) REFERENCES sales.ventas(codventa);


--
-- TOC entry 5089 (class 2606 OID 232048)
-- Name: pedidoventa pedidoventa_codcliente_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.pedidoventa
    ADD CONSTRAINT pedidoventa_codcliente_fkey FOREIGN KEY (codcliente) REFERENCES referential.cliente(codcliente);


--
-- TOC entry 5090 (class 2606 OID 232053)
-- Name: pedidoventa pedidoventa_codestmov_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.pedidoventa
    ADD CONSTRAINT pedidoventa_codestmov_fkey FOREIGN KEY (codestmov) REFERENCES referential.estadomovimiento(codestmov);


--
-- TOC entry 5091 (class 2606 OID 232058)
-- Name: pedidoventa pedidoventa_codmoneda_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.pedidoventa
    ADD CONSTRAINT pedidoventa_codmoneda_fkey FOREIGN KEY (codmoneda) REFERENCES referential.moneda(codmoneda);


--
-- TOC entry 5092 (class 2606 OID 232063)
-- Name: pedidoventa pedidoventa_codsucursal_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.pedidoventa
    ADD CONSTRAINT pedidoventa_codsucursal_fkey FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5093 (class 2606 OID 232068)
-- Name: pedidoventa pedidoventa_codtipocomprobante_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.pedidoventa
    ADD CONSTRAINT pedidoventa_codtipocomprobante_fkey FOREIGN KEY (codtipocomprobante) REFERENCES referential.tipocomprobante(codtipocomprobante);


--
-- TOC entry 5094 (class 2606 OID 232073)
-- Name: pedidoventa pedidoventa_codvendedor_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.pedidoventa
    ADD CONSTRAINT pedidoventa_codvendedor_fkey FOREIGN KEY (codvendedor) REFERENCES referential.vendedor(codvendedor);


--
-- TOC entry 5095 (class 2606 OID 232078)
-- Name: pedidoventadet pedidoventadet_codpedidov_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.pedidoventadet
    ADD CONSTRAINT pedidoventadet_codpedidov_fkey FOREIGN KEY (codpedidov) REFERENCES sales.pedidoventa(codpedidov);


--
-- TOC entry 5096 (class 2606 OID 232083)
-- Name: pedidoventadet pedidoventadet_codproducto_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.pedidoventadet
    ADD CONSTRAINT pedidoventadet_codproducto_fkey FOREIGN KEY (codproducto) REFERENCES referential.producto(codproducto);


--
-- TOC entry 5097 (class 2606 OID 232088)
-- Name: presupuestoventa presupuestoventa_codcliente_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.presupuestoventa
    ADD CONSTRAINT presupuestoventa_codcliente_fkey FOREIGN KEY (codcliente) REFERENCES referential.cliente(codcliente);


--
-- TOC entry 5098 (class 2606 OID 232093)
-- Name: presupuestoventa presupuestoventa_codestmov_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.presupuestoventa
    ADD CONSTRAINT presupuestoventa_codestmov_fkey FOREIGN KEY (codestmov) REFERENCES referential.estadomovimiento(codestmov);


--
-- TOC entry 5099 (class 2606 OID 232098)
-- Name: presupuestoventa presupuestoventa_codmoneda_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.presupuestoventa
    ADD CONSTRAINT presupuestoventa_codmoneda_fkey FOREIGN KEY (codmoneda) REFERENCES referential.moneda(codmoneda);


--
-- TOC entry 5100 (class 2606 OID 232103)
-- Name: presupuestoventa presupuestoventa_codpedidov_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.presupuestoventa
    ADD CONSTRAINT presupuestoventa_codpedidov_fkey FOREIGN KEY (codpedidov) REFERENCES sales.pedidoventa(codpedidov);


--
-- TOC entry 5101 (class 2606 OID 232108)
-- Name: presupuestoventa presupuestoventa_codsucursal_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.presupuestoventa
    ADD CONSTRAINT presupuestoventa_codsucursal_fkey FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5102 (class 2606 OID 232113)
-- Name: presupuestoventa presupuestoventa_codtipocomprobante_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.presupuestoventa
    ADD CONSTRAINT presupuestoventa_codtipocomprobante_fkey FOREIGN KEY (codtipocomprobante) REFERENCES referential.tipocomprobante(codtipocomprobante);


--
-- TOC entry 5103 (class 2606 OID 232118)
-- Name: presupuestoventa presupuestoventa_codvendedor_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.presupuestoventa
    ADD CONSTRAINT presupuestoventa_codvendedor_fkey FOREIGN KEY (codvendedor) REFERENCES referential.vendedor(codvendedor);


--
-- TOC entry 5104 (class 2606 OID 232123)
-- Name: presupuestoventadet presupuestoventadet_codiva_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.presupuestoventadet
    ADD CONSTRAINT presupuestoventadet_codiva_fkey FOREIGN KEY (codiva) REFERENCES referential.tipoiva(codiva);


--
-- TOC entry 5105 (class 2606 OID 232128)
-- Name: presupuestoventadet presupuestoventadet_codpresupuestoventa_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.presupuestoventadet
    ADD CONSTRAINT presupuestoventadet_codpresupuestoventa_fkey FOREIGN KEY (codpresupuestoventa) REFERENCES sales.presupuestoventa(codpresupuestoventa);


--
-- TOC entry 5106 (class 2606 OID 232133)
-- Name: presupuestoventadet presupuestoventadet_codproducto_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.presupuestoventadet
    ADD CONSTRAINT presupuestoventadet_codproducto_fkey FOREIGN KEY (codproducto) REFERENCES referential.producto(codproducto);


--
-- TOC entry 5107 (class 2606 OID 232138)
-- Name: ventas ventas_codcliente_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.ventas
    ADD CONSTRAINT ventas_codcliente_fkey FOREIGN KEY (codcliente) REFERENCES referential.cliente(codcliente);


--
-- TOC entry 5108 (class 2606 OID 232143)
-- Name: ventas ventas_codestmov_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.ventas
    ADD CONSTRAINT ventas_codestmov_fkey FOREIGN KEY (codestmov) REFERENCES referential.estadomovimiento(codestmov);


--
-- TOC entry 5109 (class 2606 OID 232148)
-- Name: ventas ventas_codmoneda_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.ventas
    ADD CONSTRAINT ventas_codmoneda_fkey FOREIGN KEY (codmoneda) REFERENCES referential.moneda(codmoneda);


--
-- TOC entry 5110 (class 2606 OID 232153)
-- Name: ventas ventas_codpresupuestoventa_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.ventas
    ADD CONSTRAINT ventas_codpresupuestoventa_fkey FOREIGN KEY (codpresupuestoventa) REFERENCES sales.presupuestoventa(codpresupuestoventa);


--
-- TOC entry 5111 (class 2606 OID 232158)
-- Name: ventas ventas_codsucursal_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.ventas
    ADD CONSTRAINT ventas_codsucursal_fkey FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5112 (class 2606 OID 232163)
-- Name: ventas ventas_codtipocomprobante_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.ventas
    ADD CONSTRAINT ventas_codtipocomprobante_fkey FOREIGN KEY (codtipocomprobante) REFERENCES referential.tipocomprobante(codtipocomprobante);


--
-- TOC entry 5113 (class 2606 OID 232168)
-- Name: ventas ventas_codvendedor_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.ventas
    ADD CONSTRAINT ventas_codvendedor_fkey FOREIGN KEY (codvendedor) REFERENCES referential.vendedor(codvendedor);


--
-- TOC entry 5114 (class 2606 OID 232173)
-- Name: ventasdet ventasdet_codiva_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.ventasdet
    ADD CONSTRAINT ventasdet_codiva_fkey FOREIGN KEY (codiva) REFERENCES referential.tipoiva(codiva);


--
-- TOC entry 5115 (class 2606 OID 232178)
-- Name: ventasdet ventasdet_codproducto_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.ventasdet
    ADD CONSTRAINT ventasdet_codproducto_fkey FOREIGN KEY (codproducto) REFERENCES referential.producto(codproducto);


--
-- TOC entry 5116 (class 2606 OID 232183)
-- Name: ventasdet ventasdet_codventa_fkey; Type: FK CONSTRAINT; Schema: sales; Owner: postgres
--

ALTER TABLE ONLY sales.ventasdet
    ADD CONSTRAINT ventasdet_codventa_fkey FOREIGN KEY (codventa) REFERENCES sales.ventas(codventa);


--
-- TOC entry 5117 (class 2606 OID 232188)
-- Name: diagnosticotecnico diagnosticotecnico_codempleado_fkey; Type: FK CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.diagnosticotecnico
    ADD CONSTRAINT diagnosticotecnico_codempleado_fkey FOREIGN KEY (codempleado) REFERENCES referential.empleado(codempleado);


--
-- TOC entry 5118 (class 2606 OID 232193)
-- Name: diagnosticotecnico diagnosticotecnico_codestmov_fkey; Type: FK CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.diagnosticotecnico
    ADD CONSTRAINT diagnosticotecnico_codestmov_fkey FOREIGN KEY (codestmov) REFERENCES referential.estadomovimiento(codestmov);


--
-- TOC entry 5119 (class 2606 OID 232198)
-- Name: diagnosticotecnico diagnosticotecnico_codsucursal_fkey; Type: FK CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.diagnosticotecnico
    ADD CONSTRAINT diagnosticotecnico_codsucursal_fkey FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5120 (class 2606 OID 232203)
-- Name: diagnosticotecnico diagnosticotecnico_codtipocomprobante_fkey; Type: FK CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.diagnosticotecnico
    ADD CONSTRAINT diagnosticotecnico_codtipocomprobante_fkey FOREIGN KEY (codtipocomprobante) REFERENCES referential.tipocomprobante(codtipocomprobante);


--
-- TOC entry 5121 (class 2606 OID 232208)
-- Name: diagnosticotecnico diagnosticotecnico_codvehiculo_fkey; Type: FK CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.diagnosticotecnico
    ADD CONSTRAINT diagnosticotecnico_codvehiculo_fkey FOREIGN KEY (codvehiculo) REFERENCES referential.vehiculo(codvehiculo);


--
-- TOC entry 5122 (class 2606 OID 232213)
-- Name: diagnosticotecnicodet diagnosticotecnicodet_coddiagnostico_fkey; Type: FK CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.diagnosticotecnicodet
    ADD CONSTRAINT diagnosticotecnicodet_coddiagnostico_fkey FOREIGN KEY (coddiagnostico) REFERENCES service.diagnosticotecnico(coddiagnostico);


--
-- TOC entry 5123 (class 2606 OID 232218)
-- Name: diagnosticotecnicodet diagnosticotecnicodet_codparte_fkey; Type: FK CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.diagnosticotecnicodet
    ADD CONSTRAINT diagnosticotecnicodet_codparte_fkey FOREIGN KEY (codparte) REFERENCES referential.partesvehiculo(codparte);


--
-- TOC entry 5124 (class 2606 OID 232223)
-- Name: registrovehiculo registrovehiculo_codcliente_fkey; Type: FK CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.registrovehiculo
    ADD CONSTRAINT registrovehiculo_codcliente_fkey FOREIGN KEY (codcliente) REFERENCES referential.cliente(codcliente);


--
-- TOC entry 5125 (class 2606 OID 232228)
-- Name: registrovehiculo registrovehiculo_codempleado_fkey; Type: FK CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.registrovehiculo
    ADD CONSTRAINT registrovehiculo_codempleado_fkey FOREIGN KEY (codempleado) REFERENCES referential.empleado(codempleado);


--
-- TOC entry 5126 (class 2606 OID 232233)
-- Name: registrovehiculo registrovehiculo_codsucursal_fkey; Type: FK CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.registrovehiculo
    ADD CONSTRAINT registrovehiculo_codsucursal_fkey FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5127 (class 2606 OID 232238)
-- Name: registrovehiculo registrovehiculo_codtipocomprobante_fkey; Type: FK CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.registrovehiculo
    ADD CONSTRAINT registrovehiculo_codtipocomprobante_fkey FOREIGN KEY (codtipocomprobante) REFERENCES referential.tipocomprobante(codtipocomprobante);


--
-- TOC entry 5128 (class 2606 OID 232243)
-- Name: registrovehiculo registrovehiculo_codvehiculo_fkey; Type: FK CONSTRAINT; Schema: service; Owner: postgres
--

ALTER TABLE ONLY service.registrovehiculo
    ADD CONSTRAINT registrovehiculo_codvehiculo_fkey FOREIGN KEY (codvehiculo) REFERENCES referential.vehiculo(codvehiculo);


--
-- TOC entry 5129 (class 2606 OID 232248)
-- Name: ajustes ajustes_codempleado_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.ajustes
    ADD CONSTRAINT ajustes_codempleado_fkey FOREIGN KEY (codempleado) REFERENCES referential.empleado(codempleado);


--
-- TOC entry 5130 (class 2606 OID 232253)
-- Name: ajustes ajustes_codmotivo_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.ajustes
    ADD CONSTRAINT ajustes_codmotivo_fkey FOREIGN KEY (codmotivo) REFERENCES referential.motivoajuste(codmotivo);


--
-- TOC entry 5131 (class 2606 OID 232258)
-- Name: ajustes ajustes_codsucursal_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.ajustes
    ADD CONSTRAINT ajustes_codsucursal_fkey FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5132 (class 2606 OID 232263)
-- Name: ajustes ajustes_codtipocomprobante_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.ajustes
    ADD CONSTRAINT ajustes_codtipocomprobante_fkey FOREIGN KEY (codtipocomprobante) REFERENCES referential.tipocomprobante(codtipocomprobante);


--
-- TOC entry 5133 (class 2606 OID 232268)
-- Name: ajustesdet ajustesdet_codajuste_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.ajustesdet
    ADD CONSTRAINT ajustesdet_codajuste_fkey FOREIGN KEY (codajuste) REFERENCES shared.ajustes(codajuste);


--
-- TOC entry 5134 (class 2606 OID 232273)
-- Name: ajustesdet ajustesdet_codproducto_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.ajustesdet
    ADD CONSTRAINT ajustesdet_codproducto_fkey FOREIGN KEY (codproducto) REFERENCES referential.producto(codproducto);


--
-- TOC entry 5135 (class 2606 OID 232278)
-- Name: notacredito notacredito_codcliente_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacredito
    ADD CONSTRAINT notacredito_codcliente_fkey FOREIGN KEY (codcliente) REFERENCES referential.cliente(codcliente);


--
-- TOC entry 5136 (class 2606 OID 232283)
-- Name: notacredito notacredito_codcompra_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacredito
    ADD CONSTRAINT notacredito_codcompra_fkey FOREIGN KEY (codcompra) REFERENCES purchase.compras(codcompra);


--
-- TOC entry 5137 (class 2606 OID 232288)
-- Name: notacredito notacredito_codempleado_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacredito
    ADD CONSTRAINT notacredito_codempleado_fkey FOREIGN KEY (codempleado) REFERENCES referential.empleado(codempleado);


--
-- TOC entry 5138 (class 2606 OID 232293)
-- Name: notacredito notacredito_codestmov_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacredito
    ADD CONSTRAINT notacredito_codestmov_fkey FOREIGN KEY (codestmov) REFERENCES referential.estadomovimiento(codestmov);


--
-- TOC entry 5139 (class 2606 OID 232298)
-- Name: notacredito notacredito_codmoneda_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacredito
    ADD CONSTRAINT notacredito_codmoneda_fkey FOREIGN KEY (codmoneda) REFERENCES referential.moneda(codmoneda);


--
-- TOC entry 5140 (class 2606 OID 232303)
-- Name: notacredito notacredito_codproveedor_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacredito
    ADD CONSTRAINT notacredito_codproveedor_fkey FOREIGN KEY (codproveedor) REFERENCES referential.proveedor(codproveedor);


--
-- TOC entry 5141 (class 2606 OID 232308)
-- Name: notacredito notacredito_codsucursal_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacredito
    ADD CONSTRAINT notacredito_codsucursal_fkey FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5142 (class 2606 OID 232313)
-- Name: notacredito notacredito_codtipocomprobante_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacredito
    ADD CONSTRAINT notacredito_codtipocomprobante_fkey FOREIGN KEY (codtipocomprobante) REFERENCES referential.tipocomprobante(codtipocomprobante);


--
-- TOC entry 5143 (class 2606 OID 232318)
-- Name: notacredito notacredito_codventa_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacredito
    ADD CONSTRAINT notacredito_codventa_fkey FOREIGN KEY (codventa) REFERENCES sales.ventas(codventa);


--
-- TOC entry 5144 (class 2606 OID 232323)
-- Name: notacredito notacredito_movimiento_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacredito
    ADD CONSTRAINT notacredito_movimiento_fkey FOREIGN KEY (movimiento) REFERENCES referential.movimiento(movimiento);


--
-- TOC entry 5145 (class 2606 OID 232328)
-- Name: notacreditodet notacreditodet_codiva_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacreditodet
    ADD CONSTRAINT notacreditodet_codiva_fkey FOREIGN KEY (codiva) REFERENCES referential.tipoiva(codiva);


--
-- TOC entry 5146 (class 2606 OID 232333)
-- Name: notacreditodet notacreditodet_codnotacredito_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacreditodet
    ADD CONSTRAINT notacreditodet_codnotacredito_fkey FOREIGN KEY (codnotacredito) REFERENCES shared.notacredito(codnotacredito);


--
-- TOC entry 5147 (class 2606 OID 232338)
-- Name: notacreditodet notacreditodet_codproducto_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.notacreditodet
    ADD CONSTRAINT notacreditodet_codproducto_fkey FOREIGN KEY (codproducto) REFERENCES referential.producto(codproducto);


--
-- TOC entry 5148 (class 2606 OID 232343)
-- Name: transferencia sucursaldestino_sucursal; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.transferencia
    ADD CONSTRAINT sucursaldestino_sucursal FOREIGN KEY (codsucursaldestino) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5149 (class 2606 OID 232348)
-- Name: transferencia sucursalorigen_sucursal; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.transferencia
    ADD CONSTRAINT sucursalorigen_sucursal FOREIGN KEY (codsucursal) REFERENCES referential.sucursal(codsucursal);


--
-- TOC entry 5150 (class 2606 OID 232353)
-- Name: transferencia transferencia_codestmov_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.transferencia
    ADD CONSTRAINT transferencia_codestmov_fkey FOREIGN KEY (codestmov) REFERENCES referential.estadomovimiento(codestmov);


--
-- TOC entry 5151 (class 2606 OID 232358)
-- Name: transferencia transferencia_codtipocomprobante_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.transferencia
    ADD CONSTRAINT transferencia_codtipocomprobante_fkey FOREIGN KEY (codtipocomprobante) REFERENCES referential.tipocomprobante(codtipocomprobante);


--
-- TOC entry 5154 (class 2606 OID 232363)
-- Name: transferenciadet transferenciadet_codproducto_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.transferenciadet
    ADD CONSTRAINT transferenciadet_codproducto_fkey FOREIGN KEY (codproducto) REFERENCES referential.producto(codproducto);


--
-- TOC entry 5155 (class 2606 OID 232368)
-- Name: transferenciadet transferenciadet_codtransferencia_fkey; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.transferenciadet
    ADD CONSTRAINT transferenciadet_codtransferencia_fkey FOREIGN KEY (codtransferencia) REFERENCES shared.transferencia(codtransferencia);


--
-- TOC entry 5152 (class 2606 OID 232373)
-- Name: transferencia ususarioautoriza_usuario; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.transferencia
    ADD CONSTRAINT ususarioautoriza_usuario FOREIGN KEY (usuarioautoriza) REFERENCES access.usuario(codusuario);


--
-- TOC entry 5153 (class 2606 OID 232378)
-- Name: transferencia ususariocarga_usuario; Type: FK CONSTRAINT; Schema: shared; Owner: postgres
--

ALTER TABLE ONLY shared.transferencia
    ADD CONSTRAINT ususariocarga_usuario FOREIGN KEY (usuariocarga) REFERENCES access.usuario(codusuario);


-- Completed on 2025-11-02 15:54:07

--
-- PostgreSQL database dump complete
--

