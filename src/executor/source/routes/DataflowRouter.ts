import express from 'express'
import { getDataflow } from '../api/ApiInterface'
import Logger from '../utils/logging/Logger'
import bodyParser from 'body-parser'
import * as Executor from '../executor/Executor'
import { HttpStatusCode } from '../utils/HttpStatusCode'

const DataflowRouter = express.Router()

DataflowRouter.use( bodyParser.json() )
DataflowRouter.use( bodyParser.urlencoded( { extended: true } ) )

DataflowRouter.all( '/:dataflowID', async ( req, res ) => {
  let dataflowResponse = undefined
  try {
    dataflowResponse = await getDataflow( req.params.dataflowID )
  } catch ( error ) {
    Logger.error( error )
    res.status( HttpStatusCode.NOT_FOUND ).send( { message: 'Could not get Dataflow from API.' } )
    return
  }

  try {
    await Executor.execute( JSON.parse( dataflowResponse.definition ), req )
  } catch ( error ) {
    Logger.error( error )
    res.status( HttpStatusCode.INTERNAL_SERVER_ERROR ).send( { error } )
    return
  }
  res.status( HttpStatusCode.OK ).send()
} )

export default DataflowRouter
