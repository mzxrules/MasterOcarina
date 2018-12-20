<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="@* | node()">
    <xsl:copy>
      <xsl:apply-templates select="@* | node()"/>
    </xsl:copy>
  </xsl:template>

  <xsl:template match="Value">
    <xsl:copy>
      <xsl:choose>
        <xsl:when test="@uihide">
          <xsl:apply-templates select="@uihide"/>
        </xsl:when>
      </xsl:choose>
      <xsl:choose>
        <xsl:when test="Data/@repeat">
          <xsl:attribute name="repeat">
            <xsl:value-of select="Data/@repeat"/>
          </xsl:attribute>
        </xsl:when>
      </xsl:choose>
      <xsl:element name="Data">
        <xsl:value-of select="Data"/>
      </xsl:element>
      <xsl:apply-templates select="Description"/>
      <xsl:apply-templates select="Comment"/>
      <xsl:choose>
        <xsl:when test="Data/@object-id">
          <xsl:element name="Meta">
            <xsl:element name="Object">
              <xsl:value-of select="Data/@object-id"/>
            </xsl:element>
          </xsl:element>
        </xsl:when>
      </xsl:choose>

    </xsl:copy>

  </xsl:template>

  <!--<xsl:template match="Variable">
    <xsl:copy>
      <xsl:apply-templates select="@altvar"/>
      <xsl:attribute name="maskType">
        <xsl:value-of select="Mask/@type"/>
      </xsl:attribute>
      <xsl:attribute name="mask">
        <xsl:value-of select="Mask"/>
      </xsl:attribute>
      <xsl:attribute name="default">
        <xsl:value-of select="Default"/>
      </xsl:attribute>
      <xsl:apply-templates select="Description"/>
      <xsl:apply-templates select="Comment"/>
      <xsl:apply-templates select="UI"/>
      <xsl:apply-templates select="Value"/>
    </xsl:copy>
  </xsl:template>-->
</xsl:stylesheet>
