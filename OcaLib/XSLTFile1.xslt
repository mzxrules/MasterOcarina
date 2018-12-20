<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>
  <xsl:strip-space elements="*"/>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="Address">
    <xsl:element name="Identifier">
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:copy>
        <xsl:apply-templates select="@reftype"/>
        <xsl:apply-templates select="@domain"/>
        <xsl:apply-templates select="node()"/>
      </xsl:copy>
    </xsl:element>

  </xsl:template>

  <xsl:template match="Offset">
    <xsl:element name="Identifier">
      <xsl:attribute name="id">
        <xsl:value-of select="@id"/>
      </xsl:attribute>
      <xsl:copy>
        <xsl:apply-templates select="node()"/>
      </xsl:copy>
    </xsl:element>
  </xsl:template>

  <!--
  <xsl:template match="File">
    <xsl:element name="Block">
      <xsl:attribute name="name">
        <xsl:value-of select="@n"/>
      </xsl:attribute>
      <xsl:apply-templates select="node()"/>
    </xsl:element>
  </xsl:template>

  <xsl:template match="Item">
    <xsl:element name="Address">
      <xsl:attribute name="id">
        <xsl:value-of select="@var"/>
      </xsl:attribute>
      <xsl:attribute name="reftype">
      <xsl:text>absolute</xsl:text>
      </xsl:attribute>
      <xsl:apply-templates select="node()"/>
    </xsl:element>

  </xsl:template>

  <xsl:template match="Version">
    <xsl:element name="Data">
      <xsl:attribute name="ver">
        <xsl:value-of select="@v"/>
      </xsl:attribute>
      <xsl:value-of select="."/>
    </xsl:element>
  </xsl:template>
  -->
</xsl:stylesheet>
